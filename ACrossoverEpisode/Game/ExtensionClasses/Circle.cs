#region Using

using System;
using System.Numerics;
using Emotion.Primitives;

#endregion

namespace EmotionPlayground.Game.ExtensionClasses
{
    public struct Circle
    {
        public float X;
        public float Y;
        public float Radius;

        #region Constructors

        public Circle(Vector2 center, float radius)
        {
            X = center.X;
            Y = center.Y;
            Radius = radius;
        }

        public Circle(Circle circle)
        {
            X = circle.X;
            Y = circle.Y;
            Radius = circle.Radius;
        }

        #endregion

        #region Properties

        public Vector2 Center
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector2 Top
        {
            get => new Vector2(X, Y + Radius);
        }

        public Vector2 Bottom
        {
            get => new Vector2(X, Y - Radius);
        }

        public Vector2 Left
        {
            get => new Vector2(X - Radius, Y);
        }

        public Vector2 Right
        {
            get => new Vector2(X + Radius, Y);
        }

        public float Circumference
        {
            get => 2 * (float) Math.PI * Radius;
        }

        public float Area
        {
            get => (float) Math.PI * (float) Math.Pow(Radius, 2);
        }

        #endregion

        public override string ToString()
        {
            return $"Circle - X:{X}, Y:{Y}, Radius: {Radius}";
        }

        #region Static Methods

        public static bool intersectsLine(Circle c, Line l)
        {
            bool anyMarginalPointInsideCircle = isPointInsideCircle(c, l.A) || isPointInsideCircle(c, l.B);
            if (anyMarginalPointInsideCircle) return true;

            double numerator = Math.Abs((l.B.Y - l.A.Y) * c.Center.X - (l.B.X - l.A.X) * c.Center.Y + l.B.X * l.A.Y - l.B.Y * l.A.X);
            double denominator = Utilities.distanceBetweenTwoPoints(l.A, l.B);
            if (denominator == 0) throw new DivideByZeroException();

            double distanceToLine = numerator / denominator;

            return anyMarginalPointInsideCircle && distanceToLine < c.Radius;
        }

        // TODO: Move to Rectangle ?
        public static bool isPointInsideRectangle(Vector2 point, Rectangle rect)
        {
            Vector2 AB = Vector2.Subtract(rect.TopLeft, rect.TopRight);
            Vector2 AM = Vector2.Subtract(rect.TopLeft, point);
            Vector2 BC = Vector2.Subtract(rect.TopRight, rect.BottomRight);
            Vector2 BM = Vector2.Subtract(rect.TopRight, point);

            float dotABAM = Vector2.Dot(AB, AM);
            float dotABAB = Vector2.Dot(AB, AB);
            float dotBCBM = Vector2.Dot(BC, BM);
            float dotBCBC = Vector2.Dot(BC, BC);

            return 0 <= dotABAM && dotABAM <= dotABAB && 0 <= dotBCBM && dotBCBM <= dotBCBC;
        }

        public static bool isPointInsideCircle(Circle c, Vector2 p)
        {
            return Utilities.distanceBetweenTwoPoints(c.Center, p) <= c.Radius;
        }

        public static bool intersectsRectangle(Circle c, Rectangle r)
        {
            return isPointInsideRectangle(c.Center, r) ||
                   intersectsLine(c, new Line(new Vector2(r.X, r.Y), new Vector2(r.X + r.Width, r.Y))) ||
                   intersectsLine(c, new Line(new Vector2(r.X + r.Width, r.Y), new Vector2(r.X + r.Width, r.Y + r.Height))) ||
                   intersectsLine(c, new Line(new Vector2(r.X + r.Width, r.Y + r.Height), new Vector2(r.X, r.Y + r.Height))) ||
                   intersectsLine(c, new Line(new Vector2(r.X, r.Y + r.Height), new Vector2(r.X, r.Y)));
        }

        #endregion
    }
}