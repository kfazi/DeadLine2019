namespace DeadLine2019.Algorithms
{
    using System;
    using System.Collections.Generic;

    public class Voronoi
    {
        public class Edge : IEquatable<Edge>
        {
            public Edge(Point2D start, Point2D end)
            {
                Start = start;
                End = end;
            }

            public Point2D Start { get; }

            public Point2D End { get; }

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

        public static IReadOnlyList<Edge> VoronoiEdges(IReadOnlyList<Delaunay.Triangle> allTriangles)
        {
            var voronoiEdgeList = new List<Edge>();

            for (var i = 0; i < allTriangles.Count; i++)
            {
                for (var j = 0; j < allTriangles.Count; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }

                    var neighborEdge = allTriangles[i].FindCommonEdgeWith(allTriangles[j]);
                    if (neighborEdge == null)
                    {
                        continue;
                    }

                    var voronoiEdge = new Edge(allTriangles[i].Center, allTriangles[j].Center);
                    if (!voronoiEdgeList.Contains(voronoiEdge))
                    {
                        voronoiEdgeList.Add(voronoiEdge);
                    }
                }
            }

            return voronoiEdgeList;
        }
    }
}