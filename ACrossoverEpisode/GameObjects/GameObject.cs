namespace EmotionPlayground.GameObjects
{
    using System.Numerics;
    using System;
    using Emotion.Primitives;

    public abstract class GameObject : Transform
    {
        private static uint _id = 0;

        protected GameObject(Vector3 position, Vector2 size) : base(position, size)
        {
            this.Id = ++_id;
        }

        protected GameObject(Vector3 position) : base(position, Vector2.Zero)
        {
            this.Id = ++_id;
        }

        public uint Id { get; }
    }
}
