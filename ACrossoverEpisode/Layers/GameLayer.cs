#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ACrossoverEpisode.Game;
using ACrossoverEpisode.Game.ExtensionClasses;
using ACrossoverEpisode.GameObjects;
using ACrossoverEpisode.Models;
using Emotion.Engine;
using Emotion.Engine.Scenography;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Sound;
using EmotionPlayground.GameObjects;
using FarseerPhysics.Dynamics;
using Newtonsoft.Json;

#endregion

namespace ACrossoverEpisode.Layers
{
    public class GameLayer : Scene
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
        public DialogBox CurrentDialog { get; set; } = null;

        /// <summary>
        /// List of timers.
        /// </summary>
        public List<TimerTask> Timers;

        public World PhysicsSim;

        public List<PhysicsUnit> PhysicsUnits = new List<PhysicsUnit>();

        #endregion

        // todo: unit
        private AnimatedTexture starAnimation;

        public GameLayer(TextFile mapFile)
        {
            // Deserialize map into model.
            LoadedMap = JsonConvert.DeserializeObject<GameMap>(mapFile.Content);

            bool breakpointHere = false;
        }

        public override void Load()
        {
            Timers = new List<TimerTask>();

            // Load assets.
            // todo: Null checks and fallbacks.
            Context.Settings.RenderSettings.ClearColor = LoadedMap.BackgroundColor;
            if (LoadedMap.BackgroundImage != null) Background = Context.AssetLoader.Get<Texture>(LoadedMap.BackgroundImage);
            BackgroundMusic = Context.AssetLoader.Get<SoundFile>(LoadedMap.Music);

            // Logic.

            // Init physics.
            PhysicsSim = new World(new Microsoft.Xna.Framework.Vector2(0, 9.5f));

            // Add floor.
            Unit floorUnit = new Unit(new Vector3(0, LoadedMap.FloorY, 0), new Vector2(LoadedMap.Size.X, 10));
            PhysicsUnits.Add(new PhysicsUnit(PhysicsSim, floorUnit, false, 0));

            // Create entities.
            Player = UnitFactory.CreatePlayer(LoadedMap.Spawn);
            Units.Add(Player);

            foreach (MapUnit u in LoadedMap.Units)
            {
                Unit newUnit = UnitFactory.CreateGeneric(u.Type, u.Spawn);

                Units.Add(newUnit);

                // There should be some flag for whether the unit should exist in the physics sim.
                PhysicsUnits.Add(new PhysicsUnit(PhysicsSim, newUnit));
            }

            // copy all units to the player object, temporary workaround.
            // todo
            ((Horseman)Player).AllUnits = Units;

            // Other init.
            // Context.SoundManager.Play(BackgroundMusic, "Main Layer").Looping = true;

            // Run start script.
            SetupScripting();
            if (!string.IsNullOrEmpty(LoadedMap.StartScript))
                Task.Run(() => Context.ScriptingEngine.RunScript(LoadedMap.StartScript));

            // todo: turn into a unit.
            starAnimation = new AnimatedTexture(
                Context.AssetLoader.Get<Texture>("star-spritesheet.png"),
                new Vector2(48, 48),
                AnimationLoopType.Normal,
                500,
                0,
                1
            );
        }

        public override void Update(float frameTime)
        {
            // Update physics.
            PhysicsSim.Step(frameTime / 1000f);

            if (Context.InputManager.IsMouseKeyHeld(Emotion.Input.MouseKeys.Left))
            {
                Unit newUnit = new Unit(new Vector3(Context.InputManager.GetMousePosition(), 0), new Vector2(10, 10));
                PhysicsUnits.Add(new PhysicsUnit(PhysicsSim, newUnit));
            }

            foreach (PhysicsUnit u in PhysicsUnits)
            {
                u.Unit.Position = u.Position;
            }

            foreach (Unit u in Units)
            {
                u.Update(frameTime);
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

            // Update the camera.
            // todo: This should happen automatically with a target camera, but it currently doesn't support offsets.
            Context.Renderer.Camera.X = Player.X + -275;
        }

        public override void Draw(Renderer renderer)
        {
            // Draw background first.
            switch (LoadedMap.BackgroundMode)
            {
                case BackgroundMode.SolidColor:
                    renderer.Render(new Vector3(Context.Renderer.Camera.X, Context.Renderer.Camera.Y, 0), Context.Renderer.Camera.Size, LoadedMap.BackgroundColor);
                    break;
                case BackgroundMode.MoveWithCamera:
                    renderer.Render(new Vector3(Context.Renderer.Camera.X, Context.Renderer.Camera.Y, 0), Context.Renderer.Camera.Size, Color.White, Background);
                    break;
                case BackgroundMode.HorizontalFillCameraHeight:
                    renderer.Render(new Vector3(0, 0, 0), new Vector2(LoadedMap.Size.X, Context.Renderer.Camera.Height), Color.White, Background, new Rectangle(Vector2.Zero, new Vector2(LoadedMap.Size.X, Background.Size.Y)));
                    break;
            }

            // Draw entities.
            foreach (Unit u in Units)
            {
                u.Draw(renderer);
            }

            // Debug draw physics units.
            foreach (PhysicsUnit u in PhysicsUnits)
            {
                renderer.Render(u.Position, u.Unit.Size, new Color(0, 135, 0, 135));
            }

            // Draw the current dialog box - if any.
            CurrentDialog?.Draw(renderer);

            // todo: Turn into units.
            string DebugFont = "debugFont.otf";
            string PixelatedFont = "Fonts/pixelated_princess/pixelated_princess.ttf";
            renderer.RenderString(Context.AssetLoader.Get<Font>(DebugFont), 17, "This game is like life: you can only go forward.", new Vector3(200, 100, 1), Color.Black);
            renderer.RenderString(Context.AssetLoader.Get<Font>(DebugFont), 17, "Your mind sees what your eyes cannot.", new Vector3(4700, 150, 1), Color.White);

            Vector3 dialogBoxPosition = new Vector3(1200, 290, 1);
            renderer.Render(dialogBoxPosition, new Vector2(550, 45), Color.Black);
            renderer.RenderOutline(dialogBoxPosition, new Vector2(550, 45), Color.White);
            renderer.RenderString(Context.AssetLoader.Get<Font>(PixelatedFont), 22, "If there's a god, I hope she's watching...", dialogBoxPosition + new Vector3(30, 7, 1), Color.White);

            renderer.Render(
                new Vector3(4950, 450, 0),
                new Vector2(48, 48),
                Color.White,
                starAnimation.Texture,
                starAnimation.CurrentFrame
            );
        }

        public override void Unload()
        {
        }

        #region Scripting API

        private void SetupScripting()
        {
            Context.ScriptingEngine.Expose("wait", (Action<int>)Wait);
            Context.ScriptingEngine.Expose("text", (Action<string>)Text);
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

        #endregion
    }
}