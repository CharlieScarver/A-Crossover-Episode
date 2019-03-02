#region Using

using System;
using System.Numerics;
using Emotion.Engine;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.Primitives;

#endregion

namespace EmotionPlayground.GameObjects
{
    public class Unit : IdTransform
    {
        /// <summary>
        /// The name of the unit. Used for debugging. Should probably be removed later unless we use it in the scripting for
        /// something.
        /// </summary>
        public string Name { get; }

        #region Stats

        /// <summary>
        /// The speed at which the unit moves. 40 feels normal for a player. This number is the physics units the unit will move
        /// per step while in motion. It might seem like a lot - but it isn't.
        /// </summary>
        public float MovementSpeed { get; protected set; }

        /// <summary>
        /// How far up the unit can jump. 60 feels normal for a player. Might seem like a lot - but it has to combat gravity.
        /// </summary>
        public float JumpHeight { get; protected set; }

        #endregion

        #region Status

        /// <summary>
        /// Whether the unit is currently standing on a solid object AKA ground.
        /// </summary>
        public virtual bool OnGround { get; protected set; }

        /// <summary>
        /// Whether the unit is in motion.
        /// </summary>
        public virtual bool IsMoving { get; protected set; }

        /// <summary>
        /// If true is facing right. Otherwise is facing left.
        /// </summary>
        public virtual bool FacingRight { get; protected set; }

        #endregion

        #region Scripts

        /// <summary>
        /// The script to run when the unit is interacted with by the player.
        /// </summary>
        public string InteractScript { get; set; }

        #endregion

        public AnimatedTexture Sprite;

        public Unit(string name, Vector3 position) : base(position)
        {
            Name = name;
        }

        public Unit(string name, Vector3 position, Vector2 size) : base(position, size)
        {
            Name = name;
        }

        public virtual void Update(float deltaTime)
        {
            Sprite?.Update(deltaTime);
        }

        public virtual void Draw(Renderer renderer)
        {
            // If no sprite - nothing to draw.
            if (Sprite == null) return;

            // Debug draw Id and name.
            renderer.RenderString(
                Context.AssetLoader.Get<Font>("debugFont.otf"),
                15,
                $"{Id}: {Name}",
                Position - new Vector3(0, 20, 0),
                Color.Black
            );

            renderer.Render(
                Position,
                Size,
                Color.White,
                FacingRight ? Sprite.Texture : Sprite.Texture.ModifyMatrix(Matrix4x4.CreateScale(-1, 1, 1)),
                Sprite.CurrentFrame
            );
        }
    }
}