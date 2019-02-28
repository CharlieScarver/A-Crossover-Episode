#region Using

using System;
using System.Numerics;

#endregion

namespace EmotionPlayground.Game.ExtensionClasses
{
    public struct Line
    {
        public Vector2 A;
        public Vector2 B;

        public Line(Vector2 a, Vector2 b)
        {
            A = a;
            B = b;
        }

        public float Width
        {
            get => (float) Math.Sqrt(Math.Pow(A.X - B.X, 2) + Math.Pow(A.Y - B.Y, 2));
        }

        public static bool intersectsAnotherLine(Line l1, Line l2)
        {
            if (l1.A.X > l2.A.X)
            {
            }
            else if (l1.A.Y > l2.A.Y)
            {
            }

            throw new NotImplementedException();
            //x1 < x < x2, assuming x1<x2, or
            //y1 < y < y2, assuming y1<y2, or
            //z1 < z < z2, assuming z1<z2
        }

        public override string ToString()
        {
            return $"Line - A: {A}, Y: {B}, Width: {Width}";
        }
    }
}