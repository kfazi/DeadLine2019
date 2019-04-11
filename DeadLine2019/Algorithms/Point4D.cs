namespace DeadLine2019.Algorithms
{
    using System;

    public class Point4D : IEquatable<Point4D>
    {
        public Point4D(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }

        public double W { get; }

        public static double Distance(Point4D a, Point4D b)
        {
            return Math.Sqrt(DistanceSquared(a, b));
        }

        public static double DistanceSquared(Point4D a, Point4D b)
        {
            var componentX = a.X - b.X;
            var componentY = a.Y - b.Y;
            var componentZ = a.Z - b.Z;
            var componentW = a.W - b.W;
            return componentX * componentX + componentY * componentY + componentZ * componentZ + componentW * componentW;
        }

        public static double Dot(Point4D a, Point4D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        public static Point4D operator +(Point4D a, Point4D b)
        {
            return new Point4D(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }

        public static Point4D operator -(Point4D a, Point4D b)
        {
            return new Point4D(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        }

        public static Point4D operator *(double s, Point4D a)
        {
            return new Point4D(s * a.X, s * a.Y, s * a.Z, s * a.W);
        }

        public static Point4D operator *(Point4D a, double s)
        {
            return s * a;
        }

        public bool Equals(Point4D other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Point4D)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return (hashCode * 397) ^ W.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z}, {W})";
        }

        public static bool operator ==(Point4D left, Point4D right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Point4D left, Point4D right)
        {
            return !Equals(left, right);
        }
    }
}