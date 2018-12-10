namespace EmotionPlayground.GameObjects
{
    using Emotion.Engine;
    using Emotion.Game.Animation;
    using Emotion.Graphics;
    using Emotion.Graphics.Batching;
    using Emotion.Graphics.Text;
    using System.Numerics;
    using EmotionPlayground.Game.ExtensionClasses;
    using EmotionPlayground.Interfaces;
    using System;
    using System.Collections.Generic;
    using Emotion.Primitives;

    public class Horseman : Unit, IUpdatable, IDrawable
    {
        // Debug/Visual constants
        private readonly float FacingArrowWidth = 50;
        private readonly float FacingArrowHeight = 25;
        private readonly float StaminaBarHeight = 25;

        // Gameplay constants
        private readonly float ActionAreaRadius = 400;
        private readonly float maxChageVelocity;

        // Properties
        private Circle ActionArea { get; set; }
        private List<Unit> Targets { get; set; }
        private Unit CurrentTarget { get; set; }

        private float ChargeVelocity { get; set; }
        public bool IsCharging { get; private set; }

        private List<Timer> Timers { get; set; }

        private Timer TargetCrosshairAnimationTimer { get; set; }
        private bool TargetCrosshairPosition { get; set; }

        public Horseman(Vector3 position, Vector2 size) : base(position, size)
        {
            this.SpriteSize = new Vector2(48, 48);
            this.Velocity = 5;
            this.MaxDashVelocity = 25;
            this.MaxStamina = 100;
            this.CurrentStamina = this.MaxStamina;

            this.maxChageVelocity = 40;
            this.ChargeVelocity = this.maxChageVelocity;

            this.Animation = new AnimatedTexture(
                Context.AssetLoader.Get<Texture>("horseman-spritesheet.png"),
                this.SpriteSize,
                AnimationLoopType.Normal,
                900,
                0,
                1
            );

            this.BoundingBox = new Rectangle(position.ToVec2(), new Vector2(24, 48));
            this.BBRelatives = new Vector2(
                (this.Size.X - this.BoundingBox.Size.X) / 2 + 5,
                (this.Size.Y - this.BoundingBox.Size.Y) / 2
            );

            this.FeetLevelRelatives = new Vector2();

            this.ActionArea = new Circle(new Vector2(position.X + this.Size.X / 2, position.Y + this.Size.Y / 2), ActionAreaRadius);
            this.Targets = new List<Unit>();

            this.TargetCrosshairAnimationTimer = new Timer(500);
            this.TargetCrosshairPosition = true;

            this.Timers = new List<Timer>();
            this.Timers.Add(this.TargetCrosshairAnimationTimer);
            foreach (Timer timer in this.Timers)
            {
                timer.Start();
            }

            Context.AssetLoader.Get<Font>("debugFont.otf");

            GLThread.ExecuteGLThread(() =>
            {
                buffer = new QuadMapBuffer(4);
            });

        }

        private void UpdateTimers(float deltaTime)
        {
            foreach (Timer timer in this.Timers)
            {
                timer.Update(deltaTime);
            }
        }

        private void ManageInput()
        {
            this.IsMoving = false;
            this.IsMovingRight = false;
            this.IsMovingLeft = false;
            //this.IsDashing = false;

            if (Context.InputManager.IsKeyDown("D") || Context.InputManager.IsKeyHeld("D"))
            {
                this.IsFacingRight = true;
                this.IsMoving = true;
                this.IsMovingRight = true;
                this.IsMovingLeft = false;
            }


            if (Context.InputManager.IsKeyDown("A") || Context.InputManager.IsKeyHeld("A"))
            {
                this.IsFacingRight = false;
                this.IsMoving = true;
                this.IsMovingRight = false;
                this.IsMovingLeft = true;
            }

            if (Context.InputManager.IsKeyHeld("E") && this.CurrentStamina > 90)
            {
                this.IsDashing = true;
                this.DashVelocity = 5;
                this.DashVelocityIncreaseStep = 1;
            }
            else if (Context.InputManager.IsKeyUp("E"))
            {
                this.IsDashing = false;
            }

            if (Context.InputManager.IsKeyDown("Space") && this.CurrentTarget != null)
            {
                this.IsCharging = true;
            }
        }

        // Adds the Charge mechanic to the basic movement handling
        private new void ManageMovement()
        {
            if (this.IsCharging)
            {
                bool targetIsOnTheRight = this.Position.X < this.CurrentTarget.Position.X;
                float distanceToTarget = Math.Abs(this.CurrentTarget.X - this.X);
                // When the distance becomes less than one "movement", the next movement equals the distance left
                if (distanceToTarget < this.ChargeVelocity)
                {
                    this.X = this.CurrentTarget.X;
                }
                else
                {
                    this.X += (targetIsOnTheRight ? this.ChargeVelocity : (this.ChargeVelocity * -1));
                }
                if (this.Position.X == this.CurrentTarget.Position.X)
                {
                    this.IsCharging = false;
                    this.CurrentTarget = null;
                }
            }
            
            if (this.Position.X > 5000)
            {
                this.IsFalling = true;
            }
        }
        
        private void ScanForTargets()
        {
            // Start with an empty list to remove Targets who have exited the Action Area
            this.Targets.Clear();

            // Remove current target if it's outside of the Action Area
            if (this.CurrentTarget != null && !Circle.intersectsRectangle(this.ActionArea, this.CurrentTarget.BoundingBox)) //new Rectangle(u.Position.Xy, u.Size)))
            {
                this.CurrentTarget = null;
            }

            // I wanted to try this Foreach okay, give a break
            MainLayer.GameObjects.ForEach(o =>
            {
                // Process only units - TODO make sure this actually works cause return my end the whole loop?
                if (!(o is Unit)) return;

                // Skip the player object
                Unit u = (Unit)o;
                if (ReferenceEquals(u, this)) return;

                // Check if the unit is inside the Action Area
                if (Circle.intersectsRectangle(this.ActionArea, u.BoundingBox)) //new Rectangle(u.Position.Xy, u.Size)))
                {
                    // If theres no Current Target, this unit is the new one
                    if (this.CurrentTarget == null)
                    {
                        this.CurrentTarget = u;
                    }
                    // If the player's moving right (prioritize movement direction)
                    else if (this.IsMovingRight)
                    {
                        // And the unit is on their right, this unit is the new Current Target
                        if (u.X > this.X)
                        {
                            this.CurrentTarget = u;
                        }
                    }
                    // If the player's moving left (prioritize movement direction)
                    else if (this.IsMovingLeft)
                    {
                        // And the unit is on their left, this unit is the new Current Target
                        if (u.X < this.X)
                        {
                            this.CurrentTarget = u;
                        }
                    }
                    // If there is a Current Target and the player is not moving right or left
                    // And the unit is closer that the Current Target, that unit is the new Curent Target
                    else if (Math.Abs(this.X - u.X) < Math.Abs(this.X - this.CurrentTarget.X))
                    {
                        this.CurrentTarget = u;
                    }
                    Targets.Add(u);
                }
            });
        }

        public override void Update(float deltaTime)
        {
            this.UpdateTimers(deltaTime);
            this.ManageInput();
            this.ManageMovement();
            
            // Don't scan for Targets when charging 
            if (!this.IsCharging)
            {
                this.ScanForTargets();
            }

            // Update Action Area position
            this.ActionArea = new Circle(new Vector2(this.Position.X + this.Size.X / 2, this.Position.Y + this.Size.Y / 2), ActionAreaRadius);

            base.Update(deltaTime);
        }

        MapBuffer buffer;

        public override void Draw(Renderer renderer)
        {
            // Draw the Facing "Arrow"
            Vector3 p1;
            Vector3 p2;
            Vector3 p3;

            if (this.IsFacingRight)
            {
                p1 = this.Position + new Vector3(this.Size.X - 25, -80, 0);
                p2 = this.Position + new Vector3(this.Size.X - 25, this.FacingArrowHeight - 80, 0);
                p3 = this.Position + new Vector3(this.Size.X + 25, (this.FacingArrowHeight / 2) - 80, 0);
            }
            else
            {
                p1 = this.Position + new Vector3(this.FacingArrowWidth - 25, -80, 0);
                p2 = this.Position + new Vector3(this.FacingArrowWidth - 25, this.FacingArrowHeight - 80, 0);
                p3 = this.Position + new Vector3(25 - this.FacingArrowWidth, (this.FacingArrowHeight / 2) - 80, 0);
            }

            buffer.Reset();
            buffer.MapNextVertex(p1, Color.Red);
            buffer.MapNextVertex(p2, Color.Red);
            buffer.MapNextVertex(p3, Color.Red);
            buffer.MapNextVertex(p3, Color.Red);
            buffer.Render();

            // Draw the Action Area 
            //renderer.RenderLine(new Vector3(this.ActionArea.Left), new Vector3(this.ActionArea.Top), Color.Red);
            //renderer.RenderLine(new Vector3(this.ActionArea.Top), new Vector3(this.ActionArea.Right), Color.Red);
            //renderer.RenderLine(new Vector3(this.ActionArea.Right), new Vector3(this.ActionArea.Bottom), Color.Red);
            //renderer.RenderLine(new Vector3(this.ActionArea.Bottom), new Vector3(this.ActionArea.Left), Color.Red);
            //renderer.RenderCircleOutline(new Vector3(this.ActionArea.Center), this.ActionArea.Radius, Color.Red, true);

            // Draw Stamina bar
            renderer.Render(this.Position + new Vector3(0, -50, 0), new Vector2(this.CurrentStamina, this.StaminaBarHeight), Color.Green);
            renderer.RenderLine(this.Position + new Vector3(90, -50, 0), this.Position + new Vector3(90, this.StaminaBarHeight - 50, 0), Color.Yellow);
            renderer.RenderLine(this.Position + new Vector3(this.MaxStamina, -50, 0), this.Position + new Vector3(this.MaxStamina, this.StaminaBarHeight - 50, 0), Color.Red);
            renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 15, "Stamina", this.Position + new Vector3(0, -48, 0), Color.Black);
            // Draw name
            renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 15, "Michael Horseman", new Vector3(this.X, this.Y - 20, this.Z), Color.Black);

            if (this.CurrentTarget != null)
            {
                // Draw current target ID
                renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 15, this.CurrentTarget.Id.ToString(), this.Position + new Vector3(-20, 0, 0), Color.Red);
            }

            // Draw crosshairs on Targets
            foreach (Unit target in this.Targets)
            {
                bool isCurrentTarget = target == this.CurrentTarget;
                Color crosshairColor = isCurrentTarget ? Color.Red : Color.Black;

                if (this.TargetCrosshairAnimationTimer.Ready)
                {
                    this.TargetCrosshairAnimationTimer.Start();
                    this.TargetCrosshairPosition = !this.TargetCrosshairPosition;
                }

                // Crosshair "animation"
                if (this.TargetCrosshairPosition)
                {
                    Circle c = new Circle(target.Center, target.Size.X / 1.5f);
                    renderer.RenderLine(new Vector3(c.Left, 0), new Vector3(c.Top, 0), crosshairColor);
                    renderer.RenderLine(new Vector3(c.Top, 0), new Vector3(c.Right, 0), crosshairColor);
                    renderer.RenderLine(new Vector3(c.Right, 0), new Vector3(c.Bottom, 0), crosshairColor);
                    renderer.RenderLine(new Vector3(c.Bottom, 0), new Vector3(c.Left, 0), crosshairColor);
                } else
                {
                    Rectangle r = new Rectangle(target.Position.ToVec2(), new Vector2(target.Size.X, target.Size.X));
                    renderer.RenderLine(new Vector3(r.X, r.Y, 0), new Vector3(r.X + r.Width, r.Y, 0), crosshairColor);
                    renderer.RenderLine(new Vector3(r.X + r.Width, r.Y, 0), new Vector3(r.X + r.Width, r.Y + r.Height, 0), crosshairColor);
                    renderer.RenderLine(new Vector3(r.X + r.Width, r.Y + r.Height, 0), new Vector3(r.X, r.Y + r.Height, 0), crosshairColor);
                    renderer.RenderLine(new Vector3(r.X, r.Y + r.Height, 0), new Vector3(r.X, r.Y, 0), crosshairColor);
                }
            }

            base.Draw(renderer);
        }
    }
}
