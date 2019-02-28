#region Using

using System.Numerics;
using ACrossoverEpisode.GameObjects;
using ACrossoverEpisode.GameObjects.Platforms;
using ACrossoverEpisode.Models;
using Emotion.Debug;
using Emotion.Engine;
using EmotionPlayground.GameObjects;
using FarseerPhysics.Dynamics;

#endregion

namespace ACrossoverEpisode.Game
{
    public static class UnitFactory
    {
        /// <summary>
        /// Creates and returns a player unit.
        /// </summary>
        /// <param name="spawn">The spawn location of the unit.</param>
        /// <param name="game">The game scene this unit belongs to.</param>
        /// <returns>A player unit.</returns>
        public static Unit CreatePlayer(Vector3 spawn, GameScene game)
        {
            return new Horseman(spawn, new Vector2(37 * 2, 42 * 2), game);
        }

        /// <summary>
        /// Create a generic entity.
        /// </summary>
        /// <param name="mapUnit">The map data of the unit to create.</param>
        /// <param name="game">The game scene this unit belongs to.</param>
        /// <returns>An entity of the specified type.</returns>
        public static Unit CreateGeneric(MapUnit mapUnit, GameScene game)
        {
            string type = mapUnit.Type?.ToLower();

            switch (type)
            {
                case "bouncer":
                    return new Bouncer(mapUnit.Spawn, new Vector2(96, 96), game)
                    {
                        InteractScript = mapUnit.InteractScript
                    };

                case "wonderer":
                    return new Wonderer(mapUnit.Spawn, new Vector2(96, 96), game)
                    {
                        InteractScript = mapUnit.InteractScript
                    };

                default:
                    Context.Log.Error($"Invalid unit of type {type}.", MessageSource.Game);
                    return null;
            }
        }

        /// <summary>
        /// Create a platform.
        /// </summary>
        /// <param name="mapData">The map data of a platform to create.</param>
        /// <param name="game">The game scene this unit belongs to.</param>
        /// <returns>A platform created from the map data.</returns>
        public static Unit CreatePlatform(MapPlatform mapData, GameScene game)
        {
            string type = mapData.Type?.ToLower();

            switch (type)
            {
                default: // TrueSolid
                    return new PhysicsUnit("platform", mapData.Position, mapData.Size, game, CollisionLayer.Walls, CollisionLayer.Entities, false, 0);

                case "jumpthroughplatform":
                    return new JumpThroughPlatform("JumpThroughPlatform", mapData.Position, mapData.Size, game);
            }
        }
    }
}