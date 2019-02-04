#region Using

using System.Numerics;
using EmotionPlayground.GameObjects;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

#endregion

namespace ACrossoverEpisode.GameObjects
{
    public sealed class PhysicsUnit
    {
        public static float PhysicsScale = 0.5f;

        /// <summary>
        /// The position of the physics object within the non-physical world.
        /// </summary>
        public Vector3 Position
        {
            get => new Vector3(PhysicsBody.Position.X / PhysicsScale - Unit.Size.X / 2, PhysicsBody.Position.Y / PhysicsScale - Unit.Size.Y / 2, Unit.Z);
        }

        public Body PhysicsBody { get; private set; }

        /// <summary>
        /// This connection of PhysicsUnit - Unit should be implemented through inheritance.
        /// </summary>
        public Unit Unit { get; private set; }

        /// <summary>
        /// Create a new physics unit.
        /// </summary>
        /// <param name="unit">The unit to copy properties from.</param>
        public PhysicsUnit(World physicsWorld, Unit unit, bool movable = true, float density = 1f, bool lockedRotation = true)
        {
            Unit = unit;
            PhysicsBody = BodyFactory.CreateRectangle(physicsWorld, unit.Width * PhysicsScale, unit.Height * PhysicsScale, density, new Vector2(unit.X * PhysicsScale, unit.Y * PhysicsScale), 0, movable ? BodyType.Dynamic : BodyType.Static, unit);
            PhysicsBody.FixedRotation = lockedRotation;
            PhysicsBody.Restitution = 1f;
            PhysicsBody.SleepingAllowed = true;
        }
    }
}