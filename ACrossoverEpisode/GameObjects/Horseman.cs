#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ACrossoverEpisode.Game;
using Emotion.Engine;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Primitives;
using EmotionPlayground.Game.ExtensionClasses;

#endregion

namespace ACrossoverEpisode.GameObjects
{
    public class Horseman : PhysicsUnit
    {
        public Horseman(Vector3 position, Vector2 size, GameScene game) : base("horseman-player", position, size, game, CollisionLayer.Entities, CollisionLayer.Walls)
        {
            MovementSpeed = 40;
            JumpHeight = 60;

            FacingRight = true;

            Sprite = new AnimatedTexture(
                Context.AssetLoader.Get<Texture>("horseman-spritesheet.png"),
                new Vector2(37, 42),
                AnimationLoopType.Normal,
                900,
                0,
                1
            );
        }

        private void UpdateTimers(float deltaTime)
        {
            // move to spell
            _targetChargeTimer?.Update(deltaTime);
            if (_targetChargeTimer != null && _targetChargeTimer.Ready)
            {
                _targetCharging = false;
                _targetChargeTimer = null;
            }

            if (_targetCharging)
            {
                float newDist = Microsoft.Xna.Framework.Vector2.Distance(expTarget.PhysicsBody.Position, PhysicsBody.Position);

                if (newDist > _expTargetDist)
                {
                    _targetCharging = false;
                    _targetChargeTimer = null;
                }
            }

            _chargeTimer?.Update(deltaTime);
            if (_chargeTimer == null || !_chargeTimer.Ready) return;
            _chargeTimer = null;
            _charging = false;
            PhysicsBody.GravityScale = 1f;
        }

        private void ManageInput()
        {
            // Horizontal movement input.
            Microsoft.Xna.Framework.Vector2 vel = PhysicsBody.LinearVelocity;
            vel.X = 0;

            if (Context.InputManager.IsKeyHeld("D"))
            {
                vel.X = MovementSpeed;
                FacingRight = true;
            }

            if (Context.InputManager.IsKeyHeld("A"))
            {
                vel.X = -MovementSpeed;
                FacingRight = false;
            }

            // Apply input velocity if movement isn't blocked.
            if (!_charging && !_targetCharging) PhysicsBody.LinearVelocity = vel;

            // Jump input.
            if (Context.InputManager.IsKeyDown("Space")) Jump();

            // temp - move to casting system.
            if (Context.InputManager.IsKeyDown("Q")) CastSpell_Charge();

            if (Context.InputManager.IsKeyDown("E")) CastSpell_TargetCharge();

            if (Context.InputManager.IsKeyDown("F")) CastSpell_PlayerInteract();

            //if (Context.InputManager.IsKeyHeld("E") && CurrentStamina > 90)
            //{
            //    IsDashing = true;
            //    DashVelocity = 5;
            //    DashVelocityIncreaseStep = 1;
            //}
            //else if (Context.InputManager.IsKeyUp("E"))
            //{
            //    IsDashing = false;
            //}

            //if (Context.InputManager.IsKeyDown("Space") && CurrentTarget != null) IsCharging = true;

            //if (Context.InputManager.IsKeyUp("Q"))
            //{
            //    if (CurrentTarget != null) CurrentTarget.IsTalking = !CurrentTarget.IsTalking;

            //    // Hide dialog boxes from other units
            //    foreach (Unit u in AllUnits)
            //    {
            //        if (u != CurrentTarget && u.IsTalking) u.IsTalking = false;
            //    }
            //}
        }

        // move to spell system

        private bool _charging;
        private int _chargeDuration = 200;
        private int _chargeDistance = 400;
        private Timer _chargeTimer;

        private void CastSpell_Charge()
        {
            // If charging then do nothing.
            if (_charging) return;

            // Convert total charge distance to physics units.
            float distanceCalc = _chargeDistance * PhysicsScale;

            // Calculate duration affected by stats.
            // The faster the unit - the faster the charge - by making the duration shorter.
            float normalMovementPerMs = distanceCalc / MovementSpeed;
            float durationStatScale = _chargeDuration * normalMovementPerMs;

            // Convert calculated total time to fraction of second.
            float timeMs = durationStatScale / 1000f;
            // V = S / T | At this point charge power is the distance in phys units per ms.
            float chargePower = distanceCalc / timeMs;

            // Calculate impulse based on facing direction.
            _charging = true;
            float chargeVelocity;
            if (FacingRight)
                chargeVelocity = chargePower;
            else
                chargeVelocity = -chargePower;

            // Set linear velocity to charge impulse.
            PhysicsBody.LinearVelocity = new Microsoft.Xna.Framework.Vector2(chargeVelocity, PhysicsBody.LinearVelocity.Y);
            PhysicsBody.GravityScale = 0.5f;

            // Set the charge timer to the total duration calculated.
            _chargeTimer = new Timer(durationStatScale);
            _chargeTimer.Start();
        }

        private bool _targetCharging = false;
        private int _targetChargeDuration = 200;
        private int _targetChargeRange = 500;
        private Timer _targetChargeTimer;

        private void CastSpell_TargetCharge()
        {
            // If charging then do nothing.
            if (_targetCharging) return;

            // Get closest target.
            PhysicsUnit target = Scene.GetTargets(this, _targetChargeRange, CollisionLayer.Entities).AsParallel().OrderBy(x => x.Distance).Select(x => x.Unit).FirstOrDefault();

            if (target == null) return;

            // Find distances.
            float distanceX = target.PhysicsBody.Position.X - PhysicsBody.Position.X;
            float distanceY = target.PhysicsBody.Position.Y - PhysicsBody.Position.Y;

            // Calculate power affected by stat and distance.
            float chargePowerX = distanceX / ((_targetChargeDuration * (distanceX / MovementSpeed)) / 1000f);
            float chargePowerY = distanceY / ((_targetChargeDuration * (distanceY / MovementSpeed)) / 1000f);

            _targetCharging = true;
            PhysicsBody.LinearVelocity = new Microsoft.Xna.Framework.Vector2(chargePowerX, chargePowerY);

            // Set trackers.
            expTarget = target;
            _expTargetDist = Microsoft.Xna.Framework.Vector2.Distance(target.PhysicsBody.Position, PhysicsBody.Position);
        }

        private int _interactRange = 150;

        private void CastSpell_PlayerInteract()
        {
            // Get closest target.
            PhysicsUnit target = Scene.GetTargets(this, _interactRange, CollisionLayer.Entities).AsParallel().OrderBy(x => x.Distance).Select(x => x.Unit).FirstOrDefault();

            if (target == null)
            {
                // TODO: Remove this and think of a better way to close the dialog box
                Scene.ExecuteScript("text(null);");
                return;
            }

            // If there is an interact script attached, execute it.
            if (!string.IsNullOrEmpty(target.InteractScript))
            {
                Scene.ExecuteScript(target.InteractScript, target);
            }
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);

            renderer.RenderCircleOutline(new Vector3(Center, Z), _targetChargeRange, Color.Red, true);
            renderer.RenderCircleOutline(new Vector3(Center, Z), _interactRange, Color.Blue, true);
        }

        public override void Update(float deltaTime)
        {
            UpdateTimers(deltaTime);
            ManageInput();

            base.Update(deltaTime);
        }


        PhysicsUnit expTarget;
        private float _expTargetDist = 0f;

        protected override bool OnContact(PhysicsUnit unit)
        {
            if (unit == expTarget && _targetCharging)
            {
                _targetCharging = false;
                expTarget = null;
            }

            return false;
        }
    }
}