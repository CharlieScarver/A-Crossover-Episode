namespace EmotionPlayground.GameObjects
{
    using Emotion.Engine;
    using Emotion.Game.Animation;
    using Emotion.Graphics;
    using Emotion.Graphics.Text;
    using System.Numerics;
    using EmotionPlayground.Interfaces;
    using Emotion.Primitives;
    using ACrossoverEpisode.GameObjects;

    public class Unit : GameObject, IUpdatable, IDrawable
    {
        protected float Velocity;
        protected float DashVelocity;
        protected float DashVelocityIncreaseStep;
        protected float MaxDashVelocity;
        protected float MaxStamina;
        protected AnimatedTexture Animation;
        protected Vector2 SpriteSize;
        protected Vector2 BBRelatives;
        protected Vector2 FeetLevelRelatives;

        public Rectangle BoundingBox;

        public bool IsFacingRight = true;
        public bool IsMoving = false;
        public bool IsMovingRight = false;
        public bool IsMovingLeft = false;
        public bool IsIdle = true;
        public bool IsFalling = false;

        public bool IsTalking = false;
        public bool IsDashing = false;
        public bool inAnimation = false;

        protected float CurrentStamina;
        protected DialogBox CurrentQuote;

        public Unit(Vector3 position) : base(position)
        {
        }

        public Unit(Vector3 position, Vector2 size) : base(position, size)
        {
        }

        public virtual void ManageMovement()
        {
            if (this.IsMovingRight)
            {
                this.X += this.Velocity;
            }
            else if (this.IsMovingLeft)
            {
                this.X -= this.Velocity;
            }

            if (this.IsDashing && this.CurrentStamina > 20)
            {
                if (this.IsFacingRight)
                {
                    this.X += this.DashVelocity;
                } 
                else
                {
                    this.X -= this.DashVelocity;
                }
                if (this.DashVelocity < this.MaxDashVelocity)
                {
                    this.DashVelocity += this.DashVelocityIncreaseStep;
                    this.DashVelocityIncreaseStep++;
                }

                this.CurrentStamina -= 4;
            }
            else
            {
                this.IsDashing = false;
            }

            if (this.CurrentStamina < this.MaxStamina)
            {
                this.CurrentStamina += 0.2f;
            }

            if (this.IsFalling)
            {
                this.Y -= this.Velocity;
            }
        }

        public virtual void Update(float deltaTime)
        {
            this.ManageMovement();
            this.Animation.Update(deltaTime);
            this.BoundingBox.X = this.Position.X + BBRelatives.X;
            this.BoundingBox.Y = this.Position.Y + BBRelatives.Y;
        }

        public virtual void Draw(Renderer renderer)
        {
            renderer.RenderString(
                Context.AssetLoader.Get<Font>("debugFont.otf"),
                15,
                this.Id.ToString(),
                this.Position,
                Color.Black
               );

            if (this.IsFacingRight)
            {
                renderer.Render(
                    this.Position,
                    this.Size,
                    Color.White,
                    this.Animation.Texture,
                    this.Animation.CurrentFrame
                );
            }
            else
            {
                renderer.Render(
                    this.Position,
                    this.Size,
                    Color.White,
                    this.Animation.Texture.ModifyMatrix(Matrix4x4.CreateScale(-1, 1, 1)),
                    this.Animation.CurrentFrame
                );
            }

            if (this.IsTalking && this.CurrentQuote != null)
            {
                this.CurrentQuote.Draw(renderer);
            }

            //if (this.IsFacingRight)
            //{
                //renderer.Render(
                //    this.Position,
                //    this.Size, // Display size coming from Transform.Size
                //    Color.White,
                //    this.Animation.Texture,
                //    this.Animation.CurrentFrame
                //);
            //}
            //else
            //{
            //    Matrix4 mirrorMatrix = Matrix4.CreateScale(-1, 1, 1);
            //    renderer.MatrixStack.Push(mirrorMatrix);
            //    renderer.Render(
            //        this.Position,
            //        this.Size, // Display size coming from Transform.Size
            //        Color.White,
            //        this.Animation.Texture,
            //        this.Animation.CurrentFrame
            //    );
            //    renderer.MatrixStack.Pop();
            //}
            
            //renderer.RenderOutline(
            //    this.BoundingBox.LocationZ(0),
            //    this.BoundingBox.Size,
            //    Color.Red
            //    );
        }
    }
}
