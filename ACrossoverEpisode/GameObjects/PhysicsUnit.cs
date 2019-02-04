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
        public Body PhysicsBody { get; private set; }

        /// <summary>
        /// This connection of PhysicsUnit - Unit should be implemented through inheritance.
        /// </summary>
        public Unit Unit { get; private set; }

        /// <summary>
        /// Create a new physics unit.
        /// </summary>
        /// <param name="unit">The unit to copy properties from.</param>
        public PhysicsUnit(World physicsWorld, Unit unit)
        {
            Unit = unit;
            PhysicsBody = BodyFactory.CreateRectangle(physicsWorld, unit.Width, unit.Height, 10f, new Vector2(unit.X, unit.Y), 0, BodyType.Dynamic, unit);
        }
    }
}