namespace EmotionPlayground.GameObjects
{
    #region Using

    using System.Numerics;
    using Emotion.Engine;
    using Emotion.Game.Animation;
    using Emotion.Graphics;
    using Emotion.Primitives;
    using EmotionPlayground.Game.ExtensionClasses;

    #endregion

    public class Wonderer : Unit
    {
        private Timer StartWalkingTimer { get; set; }
        private Timer StopWalkingTimer { get; set; }

        // TODO: Add one-time timers
        private bool Done { get; set; }

        public Wonderer(Vector3 position, Vector2 size) : base(position, size)
        {
            SpriteSize = new Vector2(48, 48);
            Animation = new AnimatedTexture(
                Context.AssetLoader.Get<Texture>("wonderer-spritesheet.png"),
                SpriteSize,
                AnimationLoopType.Normal,
                500,
                0,
                1
            );

            this.Velocity = 5;

            BoundingBox = new Rectangle(position.ToVec2(), new Vector2(24, 48));
            BBRelatives = new Vector2(
                (Size.X - BoundingBox.Size.X) / 2 + 5,
                (Size.Y - BoundingBox.Size.Y) / 2
            );

            this.StartWalkingTimer = new Timer(5000);
            this.StopWalkingTimer = new Timer(13000);

            this.StartWalkingTimer.Start();
            this.StopWalkingTimer.Stop();
        }

        public override void Update(float deltaTime)
        {
            this.StartWalkingTimer.Update(deltaTime);
            this.StopWalkingTimer.Update(deltaTime);

            if (this.StartWalkingTimer.Ready && !this.Done)
            {
                this.StopWalkingTimer.Start();
                this.Done = true;
                this.IsMovingRight = true;
            }

            if (this.StopWalkingTimer.Ready)
            {
                this.IsMovingRight = false;
            }

            base.Update(deltaTime);
        }
    }
}