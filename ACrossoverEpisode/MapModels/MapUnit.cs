#region Using

using System.Numerics;

#endregion

namespace ACrossoverEpisode.Models
{
    public class MapUnit
    {
        public string Type { get; set; }
        public Vector3 Spawn { get; set; }
        public string InteractScript { get; set; }
    }
}