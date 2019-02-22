#region Using

using System.Numerics;
using Emotion.Primitives;

#endregion

namespace EmotionPlayground.GameObjects
{
    public abstract class IdTransform : Transform
    {
        private static uint _id;

        public Vector3 CenterZ
        {
            get => new Vector3(Center.X, Center.Y, Z);
        }

        protected IdTransform(Vector3 position, Vector2 size) : base(position, size)
        {
            Id = ++_id;
        }

        protected IdTransform(Vector3 position) : base(position, Vector2.Zero)
        {
            Id = ++_id;
        }

        public uint Id { get; }
    }
}