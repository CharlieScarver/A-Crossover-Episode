#region Using

using System.Collections.Generic;
using System.Numerics;
using ACrossoverEpisode.Models;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
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

        private Texture _background;
        private Unit _player;

        #endregion

        #region Logic

        /// <summary>
        /// The list of units inhabiting the map.
        /// todo: Quadtree
        /// </summary>
        private List<Unit> _units = new List<Unit>();

        #endregion

        public GameLayer(TextFile mapFile)
        {
            // Deserialize map into model.
            LoadedMap = JsonConvert.DeserializeObject<GameMap>(string.Join("\n", mapFile.Content));

            // Create entities.
            _player = new Horseman(new Vector3(LoadedMap.Spawn, 1), new Vector2(96, 96));
            _units.Add(_player);

            // Hacks and workarounds.
            Context.Renderer.Camera.OnMove += (e, s) => { Context.Renderer.Camera.Update(); };

            bool breakpointHere = false;
        }

        public override void Load()
        {
            // Load assets.
            // todo: Null checks and fallbacks.
            _background = Context.AssetLoader.Get<Texture>(LoadedMap.BackgroundImage);

        }

        public override void Update(float frameTime)
        {
            foreach (Unit u in _units)
            {
                u.Update(frameTime);
            }

            // Update the camera.
            // todo: This should happen automatically with a target camera, but it currently doesn't support offsets.
            Context.Renderer.Camera.X = _player.X + -275;
        }

        public override void Draw(Renderer renderer)
        {
            // Draw background first.
            renderer.Render(new Vector3(0, 0, 0), Context.Renderer.Camera.Size, Color.White, _background);

            // Draw entities.
            foreach (Unit u in _units)
            {
                u.Draw(renderer);
            }
        }

        public override void Unload()
        {
        }
    }
}