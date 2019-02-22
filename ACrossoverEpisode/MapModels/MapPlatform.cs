#region Using

using System.Numerics;

#endregion

namespace ACrossoverEpisode.Models
{
    public class MapPlatform
    {
        public string Type { get; set; }
        public Vector2 Size { get; set; }
        public Vector3 Position { get; set; }
    }
}