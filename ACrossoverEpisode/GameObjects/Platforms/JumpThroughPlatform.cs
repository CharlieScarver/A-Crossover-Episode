#region Using

using System.Numerics;
using ACrossoverEpisode.Game;

#endregion

namespace ACrossoverEpisode.GameObjects.Platforms
{
    public class JumpThroughPlatform : PhysicsUnit
    {
        public JumpThroughPlatform(string uniqueName, Vector3 startPosition, Vector2 size, GameScene game) : base(uniqueName, startPosition, size, game, CollisionLayer.Walls, CollisionLayer.Entities,
            false, 0)
        {
        }

        protected override bool OnContact(PhysicsUnit unit)
        {
            return FloatToPhys(unit.Y + unit.Height) >= PhysicsBody.Position.Y;
        }
    }
}