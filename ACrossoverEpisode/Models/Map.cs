using System.Collections.Generic;
using System.Numerics;

namespace ACrossoverEpisode.Models
{
    public struct GameMap
    {
        // Meta Block
        public string Name { get; set; }
        public Vector2 Size { get; set; }

        // Player Block
        public Vector2 Spawn { get; set; }

        // Background
        public string Music { get; set; }
        public string BackgroundImage { get; set; }
        public BackgroundMode BackgroundMode { get; set; }

        // Unit Block
        public List<MapUnit> Units { get; set; }
    }
}