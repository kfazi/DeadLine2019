namespace DeadLine2019.Algorithms
{
    using System;

    public class Point3D : IEquatable<Point3D>
    {
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }

        public static double Distance(Point3D a, Point3D b)
        {
            return Math.Sqrt(DistanceSquared(a, b));
        }

        public static double DistanceSquared(Point3D a, Point3D b)
        {
            var componentX = a.X - b.X;
            var componentY = a.Y - b.Y;
            var componentZ = a.Z - b.Z;
            return componentX * componentX + componentY * componentY + componentZ * componentZ;
        }

        public static double Dot(Point3D a, Point3D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Point3D Cross(Point3D a, Point3D b)
        {
            return new Point3D(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        public static Point3D operator +(Point3D a, Point3D b)
        {
            return new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Point3D operator -(Point3D a, Point3D b)
        {
            return new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Point3D operator *(double s, Point3D a)
        {
            return new Point3D(s * a.X, s * a.Y, s * a.Z);
        }

        public static Point3D operator *(Point3D a, double s)
        {
            return s * a;
        }

        public bool Equals(Point3D other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Point3D)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                return (hashCode * 397) ^ Z.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public static bool operator ==(Point3D left, Point3D right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Point3D left, Point3D right)
        {
            return !Equals(left, right);
        }
    }
}