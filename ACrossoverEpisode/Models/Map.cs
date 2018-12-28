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

        // Background Art
        public string BackgroundImage { get; set; }
    }
}