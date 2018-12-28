#region Using

using System.Numerics;
using Emotion.Engine;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace EmotionPlayground.GameObjects
{
    public class Bouncer : Unit
    {
        public Bouncer(Vector3 position, Vector2 size) : base(position, size)
        {
            SpriteSize = new Vector2(48, 48);
            Animation = new AnimatedTexture(
                Context.AssetLoader.Get<Texture>("bouncer-spritesheet.png"),
                SpriteSize,
                AnimationLoopType.Normal,
                1200,
                0,
                1
            );

            BoundingBox = new Rectangle(position.ToVec2(), new Vector2(24, 48));
            BBRelatives = new Vector2(
                (Size.X - BoundingBox.Size.X) / 2 + 5,
                (Size.Y - BoundingBox.Size.Y) / 2
            );
        }
    }
}