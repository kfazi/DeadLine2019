namespace DeadLine2019.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Delaunay
    {
        public class Point : Point2D
        {
            public Point(double x, double y)
                : base(x, y)
            {
            }

            public Point(Point2D other)
                : base(other.X, other.Y)
            {
            }

            public List<Triangle> AdjoinTriangles { get; }= new List<Triangle>();
        }

        public class Triangle : IEquatable<Triangle>
        {
            public Triangle(Point vertex1, Point vertex2, Point vertex3)
            {
                Vertex1 = vertex1;
                Vertex2 = vertex2;
                Vertex3 = vertex3;

                var x1 = vertex1.X;
                var x2 = vertex2.X;
                var x3 = vertex3.X;
                var y1 = vertex1.Y;
                var y2 = vertex2.Y;
                var y3 = vertex3.Y;

                var x = ((y2 - y1) * (y3 * y3 - y1 * y1 + x3 * x3 - x1 * x1) - (y3 - y1) * (y2 * y2 - y1 * y1 + x2 * x2 - x1 * x1)) / (2 * (x3 - x1) * (y2 - y1) - 2 * ((x2 - x1) * (y3 - y1)));
                var y = ((x2 - x1) * (x3 * x3 - x1 * x1 + y3 * y3 - y1 * y1) - (x3 - x1) * (x2 * x2 - x1 * x1 + y2 * y2 - y1 * y1)) / (2 * (y3 - y1) * (x2 - x1) - 2 * ((y2 - y1) * (x3 - x1)));

                Center = new Point(x, y);
                Radius = Math.Sqrt(Math.Abs(vertex1.X - x) * Math.Abs(vertex1.X - x) + Math.Abs(vertex1.Y - y) * Math.Abs(vertex1.Y - y));
            }

            public Point Vertex1 { get; }

            public Point Vertex2 { get; }

            public Point Vertex3 { get; }

            public Point Center { get; }

            public double Radius { get; }

            public bool ContainsInCircumcircle(Point2D point)
            {
                var dSquared = (point.X - Center.X) * (point.X - Center.X) + (point.Y - Center.Y) * (point.Y - Center.Y);
                var radiusSquared = Radius * Radius;

                return dSquared < radiusSquared;
            }

            public Edge FindCommonEdgeWith(Triangle triangle)
            {
                var commonVertices = new List<Point>();

                if (Vertex1 == triangle.Vertex1 || Vertex1 == triangle.Vertex2 || Vertex1 == triangle.Vertex3) commonVertices.Add(Vertex1);
                if (Vertex2 == triangle.Vertex1 || Vertex2 == triangle.Vertex2 || Vertex2 == triangle.Vertex3) commonVertices.Add(Vertex2);
                if (Vertex3 == triangle.Vertex1 || Vertex3 == triangle.Vertex2 || Vertex3 == triangle.Vertex3) commonVertices.Add(Vertex3);

                if (commonVertices.Count != 2)
                {
                    return null;
                }

                var commonEdge = new Edge(commonVertices[0], commonVertices[1]);
                return commonEdge;
            }

            public bool Equals(Triangle other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(Vertex1, other.Vertex1) && Equals(Vertex2, other.Vertex2) && Equals(Vertex3, other.Vertex3)
                    || Equals(Vertex1, other.Vertex1) && Equals(Vertex2, other.Vertex3) && Equals(Vertex3, other.Vertex2)
                    || Equals(Vertex1, other.Vertex2) && Equals(Vertex2, other.Vertex1) && Equals(Vertex3, other.Vertex3)
                    || Equals(Vertex1, other.Vertex2) && Equals(Vertex2, other.Vertex3) && Equals(Vertex3, other.Vertex1)
                    || Equals(Vertex1, other.Vertex3) && Equals(Vertex2, other.Vertex1) && Equals(Vertex3, other.Vertex2)
                    || Equals(Vertex1, other.Vertex3) && Equals(Vertex2, other.Vertex2) && Equals(Vertex3, other.Vertex1);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Triangle)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Vertex1 != null ? Vertex1.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (Vertex2 != null ? Vertex2.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Vertex3 != null ? Vertex3.GetHashCode() : 0);
                    return hashCode;
                }
            }

            public static bool operator ==(Triangle left, Triangle right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Triangle left, Triangle right)
            {
                return !Equals(left, right);
            }
        }

        public class Edge : IEquatable<Edge>
        {
            public Edge(Point start, Point end)
            {
                Start = start;
                End = end;
            }

            public Point Start { get; }

            public Point End { get; }

            public bool ContainsVertex(Point point)
            {
                return Start == point || End == point;
            }

            public bool Equals(Edge other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(Start, other.Start) && Equals(End, other.End)
                    || Equals(Start, other.End) && Equals(End, other.Start);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Edge)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Start != null ? Start.GetHashCode() : 0) * 397) ^ (End != null ? End.GetHashCode() : 0);
                }
            }

            public static bool operator ==(Edge left, Edge right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Edge left, Edge right)
            {
                return !Equals(left, right);
            }
        }

        public static IReadOnlyList<Edge> DelaunayEdges(Triangle superTriangle, IReadOnlyList<Triangle> delaunayTriangles)
        {
            var delaunayEdges = new List<Edge>();

            foreach (var delaunayTriangle in delaunayTriangles)
            {
                var edge1 = new Edge(delaunayTriangle.Vertex1, delaunayTriangle.Vertex2);
                var edge2 = new Edge(delaunayTriangle.Vertex2, delaunayTriangle.Vertex3);
                var edge3 = new Edge(delaunayTriangle.Vertex3, delaunayTriangle.Vertex1);
                if (!edge1.ContainsVertex(superTriangle.Vertex1) && !edge1.ContainsVertex(superTriangle.Vertex2) && !edge1.ContainsVertex(superTriangle.Vertex3))
                {
                    delaunayEdges.Add(edge1);
                }

                if (!edge2.ContainsVertex(superTriangle.Vertex1) && !edge2.ContainsVertex(superTriangle.Vertex2) && !edge2.ContainsVertex(superTriangle.Vertex3))
                {
                    delaunayEdges.Add(edge2);
                }

                if (!edge3.ContainsVertex(superTriangle.Vertex1) && !edge3.ContainsVertex(superTriangle.Vertex2) && !edge3.ContainsVertex(superTriangle.Vertex3))
                {
                    delaunayEdges.Add(edge3);
                }
            }

            return delaunayEdges;
        }

        /// <summary>
        /// Expects points are ordered. OrderBy(point => point.X).ThenBy(point => point.Y)
        /// </summary>
        public static IReadOnlyList<Triangle> Triangulate(Triangle superTriangle, IReadOnlyList<Point> triangulationPoints)
        {
            var delaunayTriangles = new List<Triangle>();

            var openTriangles = new List<Triangle>();
            var closedTriangles = new List<Triangle>();

            openTriangles.Add(superTriangle);

            for (var i = 0; i < triangulationPoints.Count; i++)
            {
                var polygon = new List<Edge>();

                for (var j = openTriangles.Count - 1; j >= 0; j--)
                {
                    var dx = triangulationPoints.ElementAt(i).X - openTriangles[j].Center.X;

                    if (dx > 0.0 && dx * dx > openTriangles[j].Radius * openTriangles[j].Radius)
                    {
                        closedTriangles.Add(openTriangles[j]);
                        openTriangles.RemoveAt(j);
                        continue;
                    }

                    if (!openTriangles[j].ContainsInCircumcircle(triangulationPoints.ElementAt(i)))
                    {
                        continue;
                    }

                    polygon.Add(new Edge(openTriangles[j].Vertex1, openTriangles[j].Vertex2));
                    polygon.Add(new Edge(openTriangles[j].Vertex2, openTriangles[j].Vertex3));
                    polygon.Add(new Edge(openTriangles[j].Vertex3, openTriangles[j].Vertex1));

                    openTriangles[j].Vertex1.AdjoinTriangles.Remove(openTriangles[j]);
                    openTriangles[j].Vertex2.AdjoinTriangles.Remove(openTriangles[j]);
                    openTriangles[j].Vertex3.AdjoinTriangles.Remove(openTriangles[j]);

                    openTriangles.RemoveAt(j);
                }

                for (var j = polygon.Count - 2; j >= 0; j--)
                {
                    for (var k = polygon.Count - 1; k >= j + 1; k--)
                    {
                        if (polygon[j] != polygon[k])
                        {
                            continue;
                        }

                        polygon.RemoveAt(k);
                        polygon.RemoveAt(j);
                        k--;
                    }
                }

                foreach (var edge in polygon)
                {
                    var triangle = new Triangle(edge.Start, edge.End, triangulationPoints[i]);
                    if (double.IsNaN(triangle.Center.X) || double.IsNaN(triangle.Center.Y))
                    {
                        continue;
                    }

                    openTriangles.Add(triangle);

                    if (!triangulationPoints[i].AdjoinTriangles.Contains(triangle))
                    {
                        triangulationPoints[i].AdjoinTriangles.Add(triangle);
                    }

                    if (!edge.Start.AdjoinTriangles.Contains(triangle))
                    {
                        edge.Start.AdjoinTriangles.Add(triangle);
                    }

                    if (!edge.End.AdjoinTriangles.Contains(triangle))
                    {
                        edge.End.AdjoinTriangles.Add(triangle);
                    }
                }
            }

            foreach (var triangle in openTriangles)
            {
                delaunayTriangles.Add(triangle);
            }

            foreach (var triangle in closedTriangles)
            {
                delaunayTriangles.Add(triangle);
            }

            return delaunayTriangles;
        }

        public static Triangle SuperTriangle(IReadOnlyList<Point> triangulationPoints)
        {
            var m = triangulationPoints[0].X;

            for (var i = 1; i < triangulationPoints.Count; i++)
            {
                var xAbs = Math.Abs(triangulationPoints[i].X);
                var yAbs = Math.Abs(triangulationPoints[i].Y);
                m = Math.Max(m, Math.Max(xAbs, yAbs));
            }

            var sp1 = new Point(100 * m, 0);
            var sp2 = new Point(0, 100 * m);
            var sp3 = new Point(-100 * m, -100 * m);

            return new Triangle(sp1, sp2, sp3);
        }
    }
}