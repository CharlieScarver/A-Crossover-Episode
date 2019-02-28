#region Using

using System.Numerics;
using ACrossoverEpisode.Game;
using ACrossoverEpisode.GameObjects;
using Emotion.Engine;
using Emotion.Game.Animation;
using Emotion.Graphics;
using EmotionPlayground.Game.ExtensionClasses;

#endregion

namespace EmotionPlayground.GameObjects
{
    #region Using

    #endregion

    public class Wonderer : PhysicsUnit
    {
        private Timer StartWalkingTimer { get; set; }
        private Timer StopWalkingTimer { get; set; }

        // TODO: Add one-time timers
        private bool Done { get; set; }

        public Wonderer(Vector3 position, Vector2 size, GameScene game) : base("wonderer-npc", position, size, game, CollisionLayer.Entities, CollisionLayer.Walls)
        {
            Sprite = new AnimatedTexture(
                Context.AssetLoader.Get<Texture>("wonderer-spritesheet.png"),
                new Vector2(48, 48),
                AnimationLoopType.Normal,
                500,
                0,
                1
            );

            MovementSpeed = 5;

            StartWalkingTimer = new Timer(5000);
            StopWalkingTimer = new Timer(13000);

            StartWalkingTimer.Start();
            StopWalkingTimer.Stop();

            //this.CurrentQuote = new DialogBox("Blizzard will rage over the pyramids.");
        }

        public override void Update(float deltaTime)
        {
            StartWalkingTimer.Update(deltaTime);
            StopWalkingTimer.Update(deltaTime);

            if (StartWalkingTimer.Ready && !Done)
            {
                StopWalkingTimer.Start();
                Done = true;
                //this.IsMovingRight = true;
            }

            if (StopWalkingTimer.Ready)
            {
                //this.IsMovingRight = false;
            }

            base.Update(deltaTime);
        }
    }
}