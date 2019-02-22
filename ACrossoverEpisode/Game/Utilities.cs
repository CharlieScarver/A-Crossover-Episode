#region Using

using System;
using System.Numerics;

#endregion

namespace EmotionPlayground.Game
{
    public static class Utilities
    {
        public static float distanceBetweenTwoPoints(Vector2 p1, Vector2 p2)
        {
            return (float) Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
    }
}