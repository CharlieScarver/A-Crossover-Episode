#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Engine;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using EmotionPlayground.Game.ExtensionClasses;
using EmotionPlayground.Interfaces;

#endregion

namespace EmotionPlayground.GameObjects
{
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

        // todo: Move out of here.
        public List<Unit> AllUnits { get; set; } = new List<Unit>();

        public Horseman(Vector3 position, Vector2 size) : base(position, size)
        {
            SpriteSize = new Vector2(48, 48);
            Velocity = 5;
            MaxDashVelocity = 25;
            MaxStamina = 100;
            CurrentStamina = MaxStamina;

            maxChageVelocity = 40;
            ChargeVelocity = maxChageVelocity;

            Animation = new AnimatedTexture(
                Context.AssetLoader.Get<Texture>("horseman-spritesheet.png"),
                SpriteSize,
                AnimationLoopType.Normal,
                900,
                0,
                1
            );

            BoundingBox = new Rectangle(position.ToVec2(), new Vector2(24, 48));
            BBRelatives = new Vector2(
                (Size.X - BoundingBox.Size.X) / 2 + 5,
                (Size.Y - BoundingBox.Size.Y) / 2
            );

            FeetLevelRelatives = new Vector2();

            ActionArea = new Circle(new Vector2(position.X + Size.X / 2, position.Y + Size.Y / 2), ActionAreaRadius);
            Targets = new List<Unit>();

            TargetCrosshairAnimationTimer = new Timer(500);
            TargetCrosshairPosition = true;

            Timers = new List<Timer>();
            Timers.Add(TargetCrosshairAnimationTimer);
            foreach (Timer timer in Timers)
            {
                timer.Start();
            }

            Context.AssetLoader.Get<Font>("debugFont.otf");
        }

        private void UpdateTimers(float deltaTime)
        {
            foreach (Timer timer in Timers)
            {
                timer.Update(deltaTime);
            }
        }

        private void ManageInput()
        {
            IsMoving = false;
            IsMovingRight = false;
            IsMovingLeft = false;
            //this.IsDashing = false;

            if (Context.InputManager.IsKeyDown("D") || Context.InputManager.IsKeyHeld("D"))
            {
                IsFacingRight = true;
                IsMoving = true;
                IsMovingRight = true;
                IsMovingLeft = false;
            }


            if (Context.InputManager.IsKeyDown("A") || Context.InputManager.IsKeyHeld("A"))
            {
                IsFacingRight = false;
                IsMoving = true;
                IsMovingRight = false;
                IsMovingLeft = true;
            }

            if (Context.InputManager.IsKeyHeld("E") && CurrentStamina > 90)
            {
                IsDashing = true;
                DashVelocity = 5;
                DashVelocityIncreaseStep = 1;
            }
            else if (Context.InputManager.IsKeyUp("E"))
            {
                IsDashing = false;
            }

            if (Context.InputManager.IsKeyDown("Space") && CurrentTarget != null) IsCharging = true;

            if (Context.InputManager.IsKeyUp("Q"))
            {
                if (CurrentTarget != null) CurrentTarget.IsTalking = !CurrentTarget.IsTalking;

                // Hide dialog boxes from other units
                foreach (Unit u in AllUnits)
                {
                    if (u != CurrentTarget && u.IsTalking) u.IsTalking = false;
                }
            }
        }

        // Adds the Charge mechanic to the basic movement handling
        private new void ManageMovement()
        {
            if (IsCharging)
            {
                bool targetIsOnTheRight = Position.X < CurrentTarget.Position.X;
                float distanceToTarget = Math.Abs(CurrentTarget.X - X);
                // When the distance becomes less than one "movement", the next movement equals the distance left
                if (distanceToTarget < ChargeVelocity)
                    X = CurrentTarget.X;
                else
                    X += targetIsOnTheRight ? ChargeVelocity : ChargeVelocity * -1;
                if (Position.X == CurrentTarget.Position.X)
                {
                    IsCharging = false;
                    CurrentTarget = null;
                }
            }

            if (Position.X > 5000) IsFalling = true;
        }

        private void ScanForTargets()
        {
            // Start with an empty list to remove Targets who have exited the Action Area
            Targets.Clear();

            // Remove current target if it's outside of the Action Area
            if (CurrentTarget != null && !Circle.intersectsRectangle(ActionArea, CurrentTarget.BoundingBox)) //new Rectangle(u.Position.Xy, u.Size)))
                CurrentTarget = null;

            // I wanted to try this Foreach okay, give a break
            AllUnits.ForEach(o =>
            {
                // Skip the player object
                Unit u = o;
                if (ReferenceEquals(u, this)) return;

                // Check if the unit is inside the Action Area
                if (Circle.intersectsRectangle(ActionArea, u.BoundingBox)) //new Rectangle(u.Position.Xy, u.Size)))
                {
                    // If theres no Current Target, this unit is the new one
                    if (CurrentTarget == null)
                    {
                        CurrentTarget = u;
                    }
                    // If the player's moving right (prioritize movement direction)
                    else if (IsMovingRight)
                    {
                        // And the unit is on their right, this unit is the new Current Target
                        if (u.X > X) CurrentTarget = u;
                    }
                    // If the player's moving left (prioritize movement direction)
                    else if (IsMovingLeft)
                    {
                        // And the unit is on their left, this unit is the new Current Target
                        if (u.X < X) CurrentTarget = u;
                    }
                    // If there is a Current Target and the player is not moving right or left
                    // And the unit is closer that the Current Target, that unit is the new Curent Target
                    else if (Math.Abs(X - u.X) < Math.Abs(X - CurrentTarget.X))
                    {
                        CurrentTarget = u;
                    }

                    Targets.Add(u);
                }
            });
        }

        public override void Update(float deltaTime)
        {
            UpdateTimers(deltaTime);
            ManageInput();
            ManageMovement();

            // Don't scan for Targets when charging 
            if (!IsCharging) ScanForTargets();

            // Update Action Area position
            ActionArea = new Circle(new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y / 2), ActionAreaRadius);

            base.Update(deltaTime);
        }

        public override void Draw(Renderer renderer)
        {
            //// Draw the Facing "Arrow"
            //Vector3 p1;
            //Vector3 p2;
            //Vector3 p3;

            //if (this.IsFacingRight)
            //{
            //    p1 = this.Position + new Vector3(this.Size.X - 25, -80, 0);
            //    p2 = this.Position + new Vector3(this.Size.X - 25, this.FacingArrowHeight - 80, 0);
            //    p3 = this.Position + new Vector3(this.Size.X + 25, (this.FacingArrowHeight / 2) - 80, 0);
            //}
            //else
            //{
            //    p1 = this.Position + new Vector3(this.FacingArrowWidth - 25, -80, 0);
            //    p2 = this.Position + new Vector3(this.FacingArrowWidth - 25, this.FacingArrowHeight - 80, 0);
            //    p3 = this.Position + new Vector3(25 - this.FacingArrowWidth, (this.FacingArrowHeight / 2) - 80, 0);
            //}

            //buffer.Reset();
            //buffer.MapNextVertex(p1, Color.Red);
            //buffer.MapNextVertex(p2, Color.Red);
            //buffer.MapNextVertex(p3, Color.Red);
            //buffer.MapNextVertex(p3, Color.Red);
            //buffer.Render();

            // Draw the Action Area 
            //renderer.RenderLine(new Vector3(this.ActionArea.Left), new Vector3(this.ActionArea.Top), Color.Red);
            //renderer.RenderLine(new Vector3(this.ActionArea.Top), new Vector3(this.ActionArea.Right), Color.Red);
            //renderer.RenderLine(new Vector3(this.ActionArea.Right), new Vector3(this.ActionArea.Bottom), Color.Red);
            //renderer.RenderLine(new Vector3(this.ActionArea.Bottom), new Vector3(this.ActionArea.Left), Color.Red);
            //renderer.RenderCircleOutline(new Vector3(this.ActionArea.Center), this.ActionArea.Radius, Color.Red, true);

            // Draw Stamina bar
            renderer.Render(Position + new Vector3(0, -50, 0), new Vector2(CurrentStamina, StaminaBarHeight), Color.Green);
            renderer.RenderLine(Position + new Vector3(90, -50, 0), Position + new Vector3(90, StaminaBarHeight - 50, 0), Color.Yellow);
            renderer.RenderLine(Position + new Vector3(MaxStamina, -50, 0), Position + new Vector3(MaxStamina, StaminaBarHeight - 50, 0), Color.Red);
            renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 15, "Stamina", Position + new Vector3(0, -48, 0), Color.Black);
            // Draw name
            renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 15, "Michael Horseman", new Vector3(X, Y - 20, Z), Color.Black);

            if (CurrentTarget != null) renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 15, CurrentTarget.Id.ToString(), Position + new Vector3(-20, 0, 0), Color.Red);

            // Draw crosshairs on Targets
            foreach (Unit target in Targets)
            {
                bool isCurrentTarget = target == CurrentTarget;
                Color crosshairColor = isCurrentTarget ? Color.Red : Color.Black;

                if (TargetCrosshairAnimationTimer.Ready)
                {
                    TargetCrosshairAnimationTimer.Start();
                    TargetCrosshairPosition = !TargetCrosshairPosition;
                }

                // Crosshair "animation"
                if (TargetCrosshairPosition)
                {
                    Circle c = new Circle(target.Center, target.Size.X / 1.5f);
                    renderer.RenderLine(new Vector3(c.Left, target.Z), new Vector3(c.Top, target.Z), crosshairColor);
                    renderer.RenderLine(new Vector3(c.Top, target.Z), new Vector3(c.Right, target.Z), crosshairColor);
                    renderer.RenderLine(new Vector3(c.Right, target.Z), new Vector3(c.Bottom, target.Z), crosshairColor);
                    renderer.RenderLine(new Vector3(c.Bottom, target.Z), new Vector3(c.Left, target.Z), crosshairColor);
                }
                else
                {
                    Rectangle r = new Rectangle(target.Position.ToVec2(), new Vector2(target.Size.X, target.Size.X));
                    renderer.RenderLine(new Vector3(r.X, r.Y, target.Z), new Vector3(r.X + r.Width, r.Y, target.Z), crosshairColor);
                    renderer.RenderLine(new Vector3(r.X + r.Width, r.Y, target.Z), new Vector3(r.X + r.Width, r.Y + r.Height, target.Z), crosshairColor);
                    renderer.RenderLine(new Vector3(r.X + r.Width, r.Y + r.Height, target.Z), new Vector3(r.X, r.Y + r.Height, target.Z), crosshairColor);
                    renderer.RenderLine(new Vector3(r.X, r.Y + r.Height, target.Z), new Vector3(r.X, r.Y, target.Z), crosshairColor);
                }
            }

            base.Draw(renderer);
        }
    }
}