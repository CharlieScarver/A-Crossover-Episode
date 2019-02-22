#region Using

using System;
using System.Numerics;
using ACrossoverEpisode.Game;
using EmotionPlayground.GameObjects;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

#endregion

namespace ACrossoverEpisode.GameObjects
{
    /// <inheritdoc />
    public class PhysicsUnit : Unit
    {
        /// <summary>
        /// The physics simulation scale.
        /// </summary>
        public static float PhysicsScale = 0.1f;

        #region Overrides

        /// <summary>
        /// The X position of the physics object within the non-physical world.
        /// </summary>
        public override float X
        {
            get
            {
                // Convert physics position to actual and move from center to top left origin.
                float physToNormal = PhysicsBody.Position.X / PhysicsScale;
                return physToNormal - Width / 2f;
            }
        }

        /// <summary>
        /// The Y position of the physics object within the non-physical world.
        /// </summary>
        public override float Y
        {
            get
            {
                // Convert physics position to actual and move from center to top left origin.
                float physToNormal = PhysicsBody.Position.Y / PhysicsScale;
                return physToNormal - Height / 2f;
            }
        }

        /// <inheritdoc />
        public override bool IsMoving
        {
            get => PhysicsBody.LinearVelocity.X != 0 || PhysicsBody.LinearVelocity.Y != 0;
        }

        /// <inheritdoc />
        public override bool OnGround
        {
            get => PhysicsBody.LinearVelocity.Y == 0;
        }

        #endregion

        /// <summary>
        /// The scene this unit belongs to.
        /// </summary>
        public GameScene Scene { get; private set; }

        /// <summary>
        /// A Farseer Physics body.
        /// </summary>
        public Body PhysicsBody { get; private set; }

        /// <summary>
        /// Which collision layer this unit belongs to.
        /// </summary>
        public CollisionLayer MemberOf { get; set; }

        /// <summary>
        /// Which collision layers this unit collides with.
        /// </summary>
        public CollisionLayer CollidesWith { get; set; }

        /// <summary>
        /// Create a new physics unit.
        /// </summary>
        /// <param name="uniqueName">Name for the unit.</param>
        /// <param name="startPosition">Starting position of the unit.</param>
        /// <param name="size">Size of the unit.</param>
        /// <param name="game">The game scane which this unit is a part of.</param>
        /// <param name="member">The collision layer this physics unit is a part of.</param>
        /// <param name="with">The collision layer/s this physics unit should physically collide with.</param>
        /// <param name="movable">Whether the unit is movable.</param>
        /// <param name="density">The density of the unit. Whatever that means.</param>
        /// <param name="lockedRotation">
        /// Whether the unit shouldn't rotate. As drawing rotation is a bit yiffy and overall physics
        /// rotation is weird maybe leave this as false for now.
        /// </param>
        public PhysicsUnit(string uniqueName, Vector3 startPosition, Vector2 size, GameScene game, CollisionLayer member, CollisionLayer with, bool movable = true, float density = 1f,
            bool lockedRotation = true) : base(uniqueName, startPosition, size)
        {
            Scene = game;

            Microsoft.Xna.Framework.Vector2 physSize = VecToPhysVec(size);
            Microsoft.Xna.Framework.Vector2 physLoc = VecToPhysVec(startPosition);
            physLoc = new Microsoft.Xna.Framework.Vector2(physLoc.X + physSize.X / 2, physLoc.Y + physSize.Y / 2);

            PhysicsBody = BodyFactory.CreateRectangle(game.PhysicsSim, physSize.X, physSize.Y, density, physLoc, this);
            PhysicsBody.BodyType = movable ? BodyType.Dynamic : BodyType.Static;
            PhysicsBody.FixedRotation = lockedRotation;
            PhysicsBody.Restitution = 0f;
            PhysicsBody.Friction = 0f;
            PhysicsBody.SleepingAllowed = true;
            MemberOf = member;
            CollidesWith = with;
            PhysicsBody.OnCollision += PhysicsBody_OnCollision;
        }

        /// <summary>
        /// Internal on collision callback. Decides whether the collision is an actual physics collision and initiates the
        /// appropriate PhysicsUnit events.
        /// </summary>
        /// <param name="fixtureA">The first unit.</param>
        /// <param name="fixtureB">The second unit.</param>
        /// <param name="contact">The contact they made.</param>
        /// <returns>Whether the collision is an actual physics collision.</returns>
        private bool PhysicsBody_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            PhysicsUnit a = (PhysicsUnit) fixtureA.Body.UserData;
            PhysicsUnit b = (PhysicsUnit) fixtureB.Body.UserData;

            // Call contact callback.
            bool ignoreCollision = OnContact(a == this ? b : a);

            // Check if a physical collision has happened.
            if (ignoreCollision || a.MemberOf != b.CollidesWith && b.MemberOf != a.CollidesWith) return false;

            // Invoke collision callback.
            OnCollision(a == this ? b : a);

            return true;
        }

        #region Commands

        public void Jump()
        {
            // Check if on ground.
            if (!OnGround) return;

            // Jump up.
            float impulse = PhysicsBody.Mass * JumpHeight;
            PhysicsBody.ApplyLinearImpulse(new Microsoft.Xna.Framework.Vector2(0, -impulse), PhysicsBody.WorldCenter);
            OnGround = false;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Convert a vector2 to a physics vector.
        /// </summary>
        /// <param name="vec">The Emotion vec2 to convert.</param>
        /// <returns>A physics vector.</returns>
        public static Microsoft.Xna.Framework.Vector2 VecToPhysVec(Vector2 vec)
        {
            return new Microsoft.Xna.Framework.Vector2(vec.X * PhysicsScale, vec.Y * PhysicsScale);
        }

        /// <summary>
        /// Convert a float to a physics float.
        /// </summary>
        /// <param name="f">The float to convert.</param>
        /// <returns>A physics float.</returns>
        public static float FloatToPhys(float f)
        {
            return f * PhysicsScale;
        }

        /// <summary>
        /// Convert a vector3 to a physics vector.
        /// </summary>
        /// <param name="vec">THe Emotion vec3 to convert.</param>
        /// <returns>A physics vector.</returns>
        public static Microsoft.Xna.Framework.Vector2 VecToPhysVec(Vector3 vec)
        {
            return new Microsoft.Xna.Framework.Vector2(vec.X * PhysicsScale, vec.Y * PhysicsScale);
        }

        /// <summary>
        /// Converts a physics vector to a vector2.
        /// </summary>
        /// <param name="vec">The physics vector to convert.</param>
        /// <returns>An Emotion vec2.</returns>
        public static Vector2 PhysVecToVec(Microsoft.Xna.Framework.Vector2 vec)
        {
            return new Vector2(vec.X / PhysicsScale, vec.Y / PhysicsScale);
        }

        #endregion

        protected virtual bool OnContact(PhysicsUnit unit)
        {
            return false;
        }

        protected virtual void OnCollision(PhysicsUnit unit)
        {

        }
    }

    public enum CollisionLayer
    {
        Walls = 1,
        Entities = 2
    }
}