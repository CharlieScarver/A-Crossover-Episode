#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACrossoverEpisode.Game.EventSystem;
using ACrossoverEpisode.Game.ExtensionClasses;
using ACrossoverEpisode.GameObjects;
using ACrossoverEpisode.Models;
using Emotion.Engine;
using Emotion.Engine.Scenography;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.Input;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Sound;
using EmotionPlayground.GameObjects;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Vector3 = System.Numerics.Vector3;
using System.Linq;

#endregion

namespace ACrossoverEpisode.Game
{
    public class GameScene : Scene
    {
        #region Properties

        /// <summary>
        /// The currently loaded map.
        /// </summary>
        public GameMap LoadedMap { get; set; }

        #endregion

        #region Assets

        public Texture Background { get; set; }
        public SoundFile BackgroundMusic { get; set; }

        public Unit Player { get; set; }

        #endregion

        #region Logic

        /// <summary>
        /// The list of units inhabiting the map.
        /// todo: Quadtree
        /// </summary>
        public List<Unit> Units { get; private set; } = new List<Unit>();

        /// <summary>
        /// The dialog box currently on screen.
        /// </summary>
        public DialogBox CurrentDialog { get; set; }

        /// <summary>
        /// List of timers started by scripts and such.
        /// </summary>
        public List<TimerTask> Timers;

        /// <summary>
        /// List of events.
        /// </summary>
        public ConcurrentBag<EventListener> Events;

        /// <summary>
        /// The physics simulation.
        /// </summary>
        public World PhysicsSim;

        #endregion

        public GameScene(TextFile mapFile)
        {
            // Deserialize map into model.
            LoadedMap = JsonConvert.DeserializeObject<GameMap>(mapFile.Content);

            bool breakpointHere = false;
        }

        public override void Load()
        {
            Timers = new List<TimerTask>();
            Events = new ConcurrentBag<EventListener>();

            // Load assets.
            // todo: Null checks and fallbacks.
            Context.Settings.RenderSettings.ClearColor = LoadedMap.BackgroundColor;
            if (LoadedMap.BackgroundImage != null) Background = Context.AssetLoader.Get<Texture>(LoadedMap.BackgroundImage);
            BackgroundMusic = Context.AssetLoader.Get<SoundFile>(LoadedMap.Music);

            // Logic.

            // Init physics.
            PhysicsSim = new World(new Vector2(0, 100f));

            // Add floor.
            Unit floorUnit = new PhysicsUnit("floor", new Vector3(0, LoadedMap.FloorY, 0), new System.Numerics.Vector2(LoadedMap.Size.X, 10), this, CollisionLayer.Walls, CollisionLayer.Entities,
                false, 0);
            Units.Add(floorUnit);

            // Create platforms.
            foreach (MapPlatform u in LoadedMap.Platforms)
            {
                Units.Add(UnitFactory.CreatePlatform(u, this));
            }

            // Create entities.
            Player = UnitFactory.CreatePlayer(LoadedMap.Spawn, this);
            Units.Add(Player);

            foreach (MapUnit u in LoadedMap.Units)
            {
                Units.Add(UnitFactory.CreateGeneric(u, this));
            }

            // Other init.
            // Context.SoundManager.Play(BackgroundMusic, "Main Layer").Looping = true;

            // Run start script.
            SetupScripting();
            if (!string.IsNullOrEmpty(LoadedMap.StartScript))
                ExecuteScript(LoadedMap.StartScript);
        }

        public override void Update(float frameTime)
        {
            // Update physics.
            PhysicsSim.Step(frameTime / 1000f);

            // debug
            if (Context.InputManager.IsMouseKeyHeld(MouseKeys.Left))
            {
                Unit newUnit = new PhysicsUnit("test", new Vector3(Context.InputManager.GetMousePosition(), 0), new System.Numerics.Vector2(10, 10), this, CollisionLayer.Entities,
                    CollisionLayer.Entities);
                Units.Add(newUnit);
            }

            // Update event listeners.
            lock (Events)
            {
                bool anyTriggered = false;
                foreach (EventListener ev in Events)
                {
                    ev.Check();
                    // If the event was triggered, execute the action.
                    if (!ev.Triggered) continue;
                    ev.OnTrigger?.Invoke();
                    anyTriggered = true;
                }
                // Clear any triggered listeners.
                if(anyTriggered) Events = new ConcurrentBag<EventListener>(Events.Where(x => !x.Triggered));
            }

            // Update pending timers.
            for (int i = 0; i < Timers.Count; i++)
            {
                Timers[i].Update(frameTime);
                if (Timers[i].Ready)
                {
                    Timers.RemoveAt(i);
                    i--;
                }
            }

            // Update units.
            foreach (Unit u in Units)
            {
                u.Update(frameTime);
            }

            // Update the camera.
            // todo: This should happen automatically with a target camera, but it currently doesn't support offsets - or vertical locks.
            Context.Renderer.Camera.X = Player.X + -275f;
        }

        public override void Draw(Renderer renderer)
        {
            // Draw background first.
            switch (LoadedMap.BackgroundMode)
            {
                case BackgroundMode.SolidColor:
                    renderer.Render(Vector3.Zero, LoadedMap.Size, LoadedMap.BackgroundColor);
                    break;
                case BackgroundMode.MoveWithCamera:
                    renderer.Render(new Vector3(Context.Renderer.Camera.X, Context.Renderer.Camera.Y, 0), Context.Renderer.Camera.Size, Color.White, Background);
                    break;
                case BackgroundMode.HorizontalFillCameraHeight:
                    renderer.Render(Vector3.Zero, new System.Numerics.Vector2(LoadedMap.Size.X, Context.Renderer.Camera.Height), Color.White, Background,
                        new Rectangle(System.Numerics.Vector2.Zero, new System.Numerics.Vector2(LoadedMap.Size.X, Background.Size.Y)));
                    break;
            }

            // Draw entities.
            foreach (Unit u in Units)
            {
                u.Draw(renderer);
                renderer.Render(u.Position, u.Size, new Color(0, 135, 0, 135));
            }

            // Draw the current dialog box - if any.
            CurrentDialog?.Draw(renderer);

            // Draw debug information.
            foreach (EventListener ev in Events)
            {
                ev.DebugDraw();
            }

            // todo: Turn into units.
            string DebugFont = "debugFont.otf";
            string PixelatedFont = "Fonts/pixelated_princess/pixelated_princess.ttf";
            renderer.RenderString(Context.AssetLoader.Get<Font>(DebugFont), 17, "This game is like life: you can only go forward.", new Vector3(200, 100, 1), Color.Black);
            renderer.RenderString(Context.AssetLoader.Get<Font>(DebugFont), 17, "Your mind sees what your eyes cannot.", new Vector3(3150 , 150, 1), Color.White);

            Vector3 dialogBoxPosition = new Vector3(1250, 300, 1);
            renderer.Render(dialogBoxPosition, new System.Numerics.Vector2(550, 45), Color.Black);
            renderer.RenderOutline(dialogBoxPosition, new System.Numerics.Vector2(550, 45), Color.White);
            renderer.RenderString(Context.AssetLoader.Get<Font>(PixelatedFont), 22, "If there's a god, I hope she's watching...", dialogBoxPosition + new Vector3(30, 7, 1), Color.White);
        }

        public override void Unload()
        {
        }

        #region Scripting API

        public Task ExecuteScript(string script, Unit me = null)
        {
            return Task.Run(() =>
            {
                ScriptEnvironment(me);
                return Context.ScriptingEngine.RunScript(script);
            });
        }

        private void SetupScripting()
        {
            Context.ScriptingEngine.Expose("wait", (Action<int>) Wait);
            Context.ScriptingEngine.Expose("text", (Action<string>) Text);
            Context.ScriptingEngine.Expose("addDistanceEvent", (Func<Unit, Vector3, float, Action, bool>) AddDistanceEvent);
        }

        private void ScriptEnvironment(Unit me)
        {
            Context.ScriptingEngine.Expose("me", me);
            Context.ScriptingEngine.Expose("player", Player);
        }

        private void Wait(int duration)
        {
            TimerTask newTask = new TimerTask(duration);
            newTask.Start();
            Timers.Add(newTask);
            newTask.Task.Wait();
        }

        private void Text(string text)
        {
            CurrentDialog = text == null ? null : new DialogBox(text);
        }

        private bool AddDistanceEvent(Unit unit, Vector3 fromPosition, float distance, Action action)
        {
            DistanceEventListener disEventListener = new DistanceEventListener(unit, fromPosition, distance, action);
            // Check if the event has happened.
            disEventListener.Check();
            // If it already has - return true and don't add it to the list.
            if (disEventListener.Triggered)
            {
                disEventListener.OnTrigger?.Invoke();
                return true;
            }

            // Add to event listeners.
            Events.Add(disEventListener);
            return false;
        }

        #endregion

        #region Targetting API

        /// <summary>
        /// Get targets around a unit in any order.
        /// </summary>
        /// <param name="unit">The unit to check around.</param>
        /// <param name="radius">The radius to check in.</param>
        /// <param name="layer">The collision layer the targets must belong to.</param>
        /// <returns>A list of targets around the unit in the collision layer specified.</returns>
        public List<Target> GetTargets(PhysicsUnit unit, float radius, CollisionLayer layer)
        {
            List<Target> units = new List<Target>();

            bool QueryCallback(Fixture fix)
            {
                // Check whether the body has user data.
                object userData = fix.Body.UserData;
                if (userData == null) return true;
                // Check whether the unit is a member of the layer we want to look in.
                PhysicsUnit foundUnit = (PhysicsUnit) userData;
                if (foundUnit.MemberOf != layer || unit == foundUnit) return true;

                // Check if the distance is within the circle.

                // Determine in which direction is the target relative to the unit.
                bool below = foundUnit.Y > unit.Y;
                bool right = foundUnit.X < unit.X;
                Vector2 corner = new Vector2(foundUnit.X + (right ? foundUnit.Width : 0), foundUnit.Y + (below ? foundUnit.Height : 0));

                // Check the distance from the center to the found unit's closest corner.
                float distance = Vector2.Distance(new Vector2(unit.Center.X, unit.Center.Y), corner);
                if (distance <= radius)
                    units.Add(new Target
                    {
                        Unit = foundUnit,
                        Distance = distance
                    });

                return true;
            }

            AABB area = new AABB(unit.PhysicsBody.Position, PhysicsUnit.FloatToPhys(radius * 2), PhysicsUnit.FloatToPhys(radius * 2));
            PhysicsSim.QueryAABB(QueryCallback, ref area);

            return units;
        }

        #endregion
    }

    public class Target
    {
        public PhysicsUnit Unit;
        public float Distance;
    }
}