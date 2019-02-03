using System.Collections.Generic;
using System.Numerics;
using Emotion.Primitives;

namespace ACrossoverEpisode.Models
{
    public struct GameMap
    {
        // Meta Block
        public string Name { get; set; }
        public Vector2 Size { get; set; }

        // Player Block
        public Vector3 Spawn { get; set; }

        // Background
        public string Music { get; set; }
        public string BackgroundImage { get; set; }
        public BackgroundMode? BackgroundMode { get; set; }
        public Color BackgroundColor { get; set; }

        // Unit Block
        public List<MapUnit> Units { get; set; }

        // Scripts
        public string StartScript { get; set; }
    }
}