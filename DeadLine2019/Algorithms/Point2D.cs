namespace DeadLine2019.Algorithms
{
    using System;

    public class Point2D : IEquatable<Point2D>
    {
        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }

        public double Y { get; }

        public static double Distance(Point2D a, Point2D b)
        {
            return Math.Sqrt(DistanceSquared(a, b));
        }

        public static double DistanceSquared(Point2D a, Point2D b)
        {
            var componentX = a.X - b.X;
            var componentY = a.Y - b.Y;
            return componentX * componentX + componentY * componentY;
        }

        public static double Dot(Point2D a, Point2D b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static Point2D operator +(Point2D a, Point2D b)
        {
            return new Point2D(a.X + b.X, a.Y + b.Y);
        }

        public static Point2D operator -(Point2D a, Point2D b)
        {
            return new Point2D(a.X - b.X, a.Y - b.Y);
        }

        public static Point2D operator *(double s, Point2D a)
        {
            return new Point2D(s * a.X, s * a.Y);
        }

        public static Point2D operator *(Point2D a, double s)
        {
            return s * a;
        }

        public bool Equals(Point2D other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Point2D)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public static bool operator ==(Point2D left, Point2D right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Point2D left, Point2D right)
        {
            return !Equals(left, right);
        }
    }
}