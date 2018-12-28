#region Using

using System.Collections.Generic;
using System.Numerics;
using ACrossoverEpisode.Game;
using ACrossoverEpisode.Models;
using Emotion.Engine;
using Emotion.Game.Animation;
using Emotion.Game.Layering;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Sound;
using EmotionPlayground.GameObjects;
using Newtonsoft.Json;

#endregion

namespace ACrossoverEpisode.Layers
{
    public class GameLayer : Layer
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
        public List<Unit> Units { get; } = new List<Unit>();

        #endregion

        // todo: unit
        private AnimatedTexture starAnimation;

        public GameLayer(TextFile mapFile)
        {
            // Deserialize map into model.
            LoadedMap = JsonConvert.DeserializeObject<GameMap>(string.Join("\n", mapFile.Content));

            bool breakpointHere = false;
        }

        public override void Load()
        {
            // Load assets.
            // todo: Null checks and fallbacks.
            Background = Context.AssetLoader.Get<Texture>(LoadedMap.BackgroundImage);
            BackgroundMusic = Context.AssetLoader.Get<SoundFile>(LoadedMap.Music);

            // Logic.

            // Create entities.
            Player = UnitFactory.CreatePlayer(LoadedMap.Spawn);
            Units.Add(Player);

            foreach (MapUnit u in LoadedMap.Units)
            {
                Units.Add(UnitFactory.CreateGeneric(u.Type, u.Spawn));
            }

            // copy all units to the player object, temporary workaround.
            // todo
            ((Horseman) Player).AllUnits = Units;

            // Hacks and workarounds.
            Context.Renderer.Camera.OnMove += (e, s) => { Context.Renderer.Camera.Update(); };

            // Other init.
            // Context.SoundManager.Play(BackgroundMusic, "Main Layer").Looping = true;

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
            foreach (Unit u in Units)
            {
                u.Update(frameTime);
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
                case BackgroundMode.MoveWithCamera:
                    renderer.Render(new Vector3(Context.Renderer.Camera.X, Context.Renderer.Camera.Y, 0), Context.Renderer.Camera.Size, Color.White, Background);
                    break;
            }

            // Draw entities.
            foreach (Unit u in Units)
            {
                u.Draw(renderer);
            }

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
    }
}