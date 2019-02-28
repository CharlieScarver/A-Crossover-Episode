#region Using

using System;

#endregion

namespace ACrossoverEpisode.Game.EventSystem
{
    /// <summary>
    /// A pending event.
    /// </summary>
    public abstract class EventListener
    {
        /// <summary>
        /// Whether the event has been triggered.
        /// </summary>
        public bool Triggered { get; protected set; } // bass_boost.wav

        /// <summary>
        /// The action to invoke once the event is triggered. Is handled by the GameScene event code.
        /// </summary>
        public Action OnTrigger { get; private set; }

        /// <summary>
        /// Create a new event.
        /// </summary>
        /// <param name="onTrigger">The action to invoke when triggered.</param>
        protected EventListener(Action onTrigger)
        {
            OnTrigger = onTrigger;
        }

        /// <summary>
        /// Check whether the event has happened.
        /// </summary>
        public abstract void Check();

        /// <summary>
        /// Draw debugging information about the event.
        /// </summary>
        public virtual void DebugDraw()
        {
        }
    }
}