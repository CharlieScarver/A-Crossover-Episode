#region Using

using System.Numerics;
using EmotionPlayground.GameObjects;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

#endregion

namespace ACrossoverEpisode.GameObjects
{
    public sealed class PhysicsUnit
    {
        public static float PhysicsScale = 0.2f;

        /// <summary>
        /// The position of the physics object within the non-physical world.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                Vector2 physVec = PhysVecToVec(PhysicsBody.Position);

                return new Vector3(physVec.X - Unit.Size.X / 2, physVec.Y - Unit.Size.Y / 2, Unit.Z);
            }
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
            Microsoft.Xna.Framework.Vector2 physVec = VecToPhysVec(unit.Size);

            Unit = unit;
            PhysicsBody = BodyFactory.CreateRectangle(physicsWorld, physVec.X, physVec.Y, density, VecToPhysVec(unit.Position));
            PhysicsBody.IsStatic = !movable;
            PhysicsBody.FixedRotation = lockedRotation;
            PhysicsBody.Restitution = 0f;
            PhysicsBody.SleepingAllowed = true;
        }

        public static Microsoft.Xna.Framework.Vector2 VecToPhysVec(Vector2 vec)
        {
            return new Microsoft.Xna.Framework.Vector2(vec.X * PhysicsScale, (vec.Y * PhysicsScale));
        }

        public static Microsoft.Xna.Framework.Vector2 VecToPhysVec(Vector3 vec)
        {
            return new Microsoft.Xna.Framework.Vector2(vec.X * PhysicsScale, (vec.Y * PhysicsScale));
        }

        public static Vector2 PhysVecToVec(Microsoft.Xna.Framework.Vector2 vec)
        {
            return new Vector2(vec.X / PhysicsScale, (vec.Y / PhysicsScale));
        }
    }
}