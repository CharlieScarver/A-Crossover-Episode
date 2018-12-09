namespace EmotionPlayground.GameObjects
{
    using Emotion.Engine;
    using Emotion.Game.Animation;
    using Emotion.Graphics;
    using Emotion.IO;
    using Emotion.Primitives;
    using System.Numerics;


    public class Bouncer : Unit
    {
        public Bouncer(Vector3 position, Vector2 size) : base(position, size)
        {
            this.SpriteSize = new Vector2(48, 48);
            this.Animation = new AnimatedTexture(
                Context.AssetLoader.Get<Texture>("bouncer-spritesheet.png"),
                this.SpriteSize,
                AnimationLoopType.Normal,
                1200,
                0,
                1
            );

            this.BoundingBox = new Rectangle(position.ToVec2(), new Vector2(24, 48));
            this.BBRelatives = new Vector2(
                (this.Size.X - this.BoundingBox.Size.X) / 2 + 5,
                (this.Size.Y - this.BoundingBox.Size.Y) / 2
            );
        }
    }
}
