#region Using

using System;
using System.Numerics;
using Emotion.Engine;
using EmotionPlayground.GameObjects;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

#endregion

namespace ACrossoverEpisode.GameObjects
{
    public sealed class PhysicsUnit
    {
        /// <summary>
        /// The physics simulation scale.
        /// </summary>
        public static float PhysicsScale = 0.2f;

        /// <summary>
        /// The position of the physics object within the non-physical world.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                Vector2 physVec = PhysVecToVec(PhysicsBody.Position);

                return new Vector3(physVec.X - Unit.Size.X / 2f, physVec.Y - Unit.Size.Y / 2f, Unit.Z);
            }
        }

        /// <summary>
        /// A Farseer Physics body.
        /// </summary>
        public Body PhysicsBody { get; private set; }

        /// <summary>
        /// This connection of PhysicsUnit - Unit should be implemented through inheritance.
        /// </summary>
        public Unit Unit { get; private set; }

        /// <summary>
        /// Which collision layer this unit belongs to.
        /// </summary>
        public CollisionLayer MemberOf { get; set; }

        /// <summary>
        /// Which collision layers this unit collides with.
        /// </summary>
        public CollisionLayer CollidesWith { get; set; }

        #region Events

        /// <summary>
        /// When two physics bodies collide. This means they physically exert forces upon each other.
        /// </summary>
        public Action<PhysicsUnit, PhysicsUnit> OnCollide;

        /// <summary>
        /// When two physics bodies make contact. This means they collided but did not physically push themselves away.
        /// </summary>
        public Action<PhysicsUnit, PhysicsUnit> OnContact;

        #endregion

        /// <summary>
        /// Create a new physics unit.
        /// </summary>
        /// <param name="physicsWorld">The simulation world this unit should be a part of.</param>
        /// <param name="unit">The unit class related to this physics unit.</param>
        /// <param name="member">The collision layer this physics unit is a part of.</param>
        /// <param name="with">The collision layer/s this physics unit should physically collide with.</param>
        /// <param name="movable">Whether the unit is movable.</param>
        /// <param name="density">The density of the unit. Whatever that means.</param>
        /// <param name="lockedRotation">Whether the unit shouldn't rotate. As drawing rotation is a bit yiffy and overall physics rotation is weird maybe leave this as false for now.</param>
        public PhysicsUnit(World physicsWorld, Unit unit, CollisionLayer member, CollisionLayer with, bool movable = true, float density = 1f, bool lockedRotation = true)
        {
            Microsoft.Xna.Framework.Vector2 physVec = VecToPhysVec(unit.Size);

            Unit = unit;
            PhysicsBody = BodyFactory.CreateRectangle(physicsWorld, physVec.X, physVec.Y, density, VecToPhysVec(unit.Position), this);
            PhysicsBody.BodyType = movable ? BodyType.Dynamic : BodyType.Static;
            PhysicsBody.FixedRotation = lockedRotation;
            PhysicsBody.Restitution = 0f;
            PhysicsBody.SleepingAllowed = true;
            MemberOf = member;
            CollidesWith = with;
            PhysicsBody.OnCollision += PhysicsBody_OnCollision;
        }

        /// <summary>
        /// Internal on collision callback. Decides whether the collision is an actual physics collision and initiates the appropriate PhysicsUnit events.
        /// </summary>
        /// <param name="fixtureA">The first unit.</param>
        /// <param name="fixtureB">The second unit.</param>
        /// <param name="contact">The contact they made.</param>
        /// <returns>Whether the collision is an actual physics collision.</returns>
        private bool PhysicsBody_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            PhysicsUnit a = (PhysicsUnit) fixtureA.Body.UserData;
            PhysicsUnit b = (PhysicsUnit) fixtureB.Body.UserData;

            // Call contact callback.
            OnContact?.Invoke(a, b);

            // Check if a physical collision has happened.
            if(a.MemberOf != b.CollidesWith && b.MemberOf != a.CollidesWith) return false;

            // Invoke collision callback.
            OnCollide?.Invoke(a, b);

            return true;
        }

        /// <summary>
        /// Convert a vector2 to a physics vector.
        /// </summary>
        /// <param name="vec">The Emotion vec2 to convert.</param>
        /// <returns>A physics vector.</returns>
        public static Microsoft.Xna.Framework.Vector2 VecToPhysVec(Vector2 vec)
        {
            return new Microsoft.Xna.Framework.Vector2(vec.X * PhysicsScale, (vec.Y * PhysicsScale));
        }

        /// <summary>
        /// Convert a vector3 to a physics vector.
        /// </summary>
        /// <param name="vec">THe Emotion vec3 to convert.</param>
        /// <returns>A physics vector.</returns>
        public static Microsoft.Xna.Framework.Vector2 VecToPhysVec(Vector3 vec)
        {
            return new Microsoft.Xna.Framework.Vector2(vec.X * PhysicsScale, (vec.Y * PhysicsScale));
        }

        /// <summary>
        /// Converts a physics vector to a vector2.
        /// </summary>
        /// <param name="vec">The physics vector to convert.</param>
        /// <returns>An Emotion vec2.</returns>
        public static Vector2 PhysVecToVec(Microsoft.Xna.Framework.Vector2 vec)
        {
            return new Vector2(vec.X / PhysicsScale, (vec.Y / PhysicsScale));
        }
    }

    public enum CollisionLayer
    {
        Walls = 1,
        Entities = 2
    }
}