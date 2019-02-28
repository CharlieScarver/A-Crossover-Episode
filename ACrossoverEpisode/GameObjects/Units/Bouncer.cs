#region Using

using System.Numerics;
using ACrossoverEpisode.Game;
using Emotion.Engine;
using Emotion.Game.Animation;
using Emotion.Graphics;

#endregion

namespace ACrossoverEpisode.GameObjects
{
    public class Bouncer : PhysicsUnit
    {
        public Bouncer(Vector3 position, Vector2 size, GameScene game) : base("bouncer-enemy", position, size, game, CollisionLayer.Entities, CollisionLayer.Walls)
        {
            Sprite = new AnimatedTexture(
                Context.AssetLoader.Get<Texture>("bouncer-spritesheet.png"),
                new Vector2(48, 48),
                AnimationLoopType.Normal,
                1200,
                0,
                1
            );
        }
    }
}