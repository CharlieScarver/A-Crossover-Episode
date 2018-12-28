#region Using

using System.Numerics;
using Emotion.Engine;
using EmotionPlayground.GameObjects;

#endregion

namespace ACrossoverEpisode.Game
{
    public static class UnitFactory
    {
        /// <summary>
        /// Creates and returns a player unit.
        /// </summary>
        /// <param name="spawn">The spawn location of the unit.</param>
        /// <returns>A player unit.</returns>
        public static Unit CreatePlayer(Vector3 spawn)
        {
            return new Horseman(spawn, new Vector2(96, 96));
        }

        /// <summary>
        /// Create a generic entity.
        /// </summary>
        /// <param name="type">The entity type. Case insensitive.</param>
        /// <param name="spawn">The spawn location for the unit.</param>
        /// <returns>An entity of the specified type.</returns>
        public static Unit CreateGeneric(string type, Vector3 spawn)
        {
            switch (type.ToLower())
            {
                case "bouncer":
                    return new Bouncer(spawn, new Vector2(96, 96));

                default:
                    Context.Log.Error($"Invalid unit of type {type}.", Emotion.Debug.MessageSource.Game);
                    return null;
            }
        }
    }
}