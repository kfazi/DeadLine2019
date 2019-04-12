namespace DeadLine2019.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharpBoostVoronoi;

    public class TerrainAnalysis
    {
        public class TerrainNode
        {
            public TerrainNode(Point2D point, bool isChoke, IReadOnlyList<TerrainNode> neighbors)
            {
                Point = point;
                IsChoke = isChoke;
                Neighbors = neighbors;
            }

            public Point2D Point { get; }

            public IReadOnlyList<TerrainNode> Neighbors { get; }

            public bool IsChoke { get; }
        }

        private class InternalNode
        {
            public Point2D Point { get; set; }

            public List<InternalNode> Neighbors { get; set; }

            public double MinDistanceToObstacleSquared { get; set; }

            public bool IsChoke { get; set; }
        }

        private class Edge
        {
            public Point2D Start { get; set; }

            public Point2D End { get; set; }
        }

        public static IReadOnlyList<TerrainNode> Analyze<TNode>(Map2D<TNode> map, Func<TNode, bool> isBlocking, int pruneDistanceToObstacleSquared, int maxDistanceToMergeSquared)
        {
            var tracingResult = ComponentLabeling.Trace(map, node => isBlocking(node) ? 1 : 0);

            var contours = new List<List<Point2D>>();
            foreach (var blob in tracingResult.Blobs)
            {
                contours.AddRange(
                    new[] { blob.ExternalContour }.Concat(blob.InternalContours).Select(
                        points => DouglasPeucker.Simplify(points.ToList(), 2 * 2, DistanceSquared)));
            }

            var edges = new List<Edge>();
            var leftBorder = new List<int>();
            var rightBorder = new List<int>();
            var topBorder = new List<int>();
            var bottomBorder = new List<int>();
            var wallsTree = new RTree<Edge>(5, 9);
            using (var voronoi = new BoostVoronoi())
            {
                foreach (var contour in contours)
                {
                    var startPoint = contour.First();
                    foreach (var point in contour.Skip(1))
                    {
                        voronoi.AddSegment((int)startPoint.X, (int)startPoint.Y, (int)point.X, (int)point.Y);
                        wallsTree.Insert(new Edge { Start = startPoint, End = point }, new RTree<Edge>.Envelope(startPoint, point));

                        if ((int)startPoint.X == 0 && (int)point.X == 0)
                        {
                            leftBorder.Add((int)startPoint.Y);
                            leftBorder.Add((int)point.Y);
                        }

                        if ((int)startPoint.X == map.Width - 1 && (int)point.X == map.Width - 1)
                        {
                            rightBorder.Add((int)startPoint.Y);
                            rightBorder.Add((int)point.Y);
                        }

                        if ((int)startPoint.Y == 0 && (int)point.Y == 0)
                        {
                            topBorder.Add((int)startPoint.X);
                            topBorder.Add((int)point.X);
                        }

                        if ((int)startPoint.Y == map.Height - 1 && (int)point.Y == map.Height - 1)
                        {
                            bottomBorder.Add((int)startPoint.X);
                            bottomBorder.Add((int)point.X);
                        }

                        startPoint = point;
                    }
                }

                AddVerticalBorder(voronoi, wallsTree, leftBorder.OrderBy(x => x).ToList(), 0, map.Height - 1);
                AddVerticalBorder(voronoi, wallsTree, rightBorder.OrderBy(x => x).ToList(), map.Width - 1, map.Height - 1);
                AddHorizontalBorder(voronoi, wallsTree, topBorder.OrderBy(x => x).ToList(), 0, map.Width - 1);
                AddHorizontalBorder(voronoi, wallsTree, bottomBorder.OrderBy(x => x).ToList(), map.Height - 1, map.Width - 1);

                var visitedTwins = new List<long>();
                voronoi.Construct();
                for (long i = 0; i < voronoi.CountEdges; i++)
                {
                    var edge = voronoi.GetEdge(i);

                    if (!edge.IsPrimary || !edge.IsFinite || visitedTwins.Contains(edge.Twin))
                    {
                        continue;
                    }

                    visitedTwins.Add(edge.Twin);

                    var start = voronoi.GetVertex(edge.Start);
                    var end = voronoi.GetVertex(edge.End);

                    if (double.IsNaN(start.X) || double.IsNaN(start.Y) || double.IsNaN(end.X) || double.IsNaN(end.Y))
                    {
                        continue;
                    }

                    if (edges.Any(
                        e => (int)e.Start.X == (int)start.X && (int)e.Start.Y == (int)start.Y &&
                             (int)e.End.X == (int)end.X && (int)e.End.Y == (int)end.Y
                             || (int)e.Start.X == (int)end.X && (int)e.Start.Y == (int)end.Y &&
                             (int)e.End.X == (int)start.X && (int)e.End.Y == (int)start.Y))
                    {
                        continue;
                    }

                    edges.Add(new Edge
                    {
                        Start = new Point2D((int)start.X, (int)start.Y),
                        End = new Point2D((int)end.X, (int)end.Y)
                    });
                }
            }

            var walkableEdges = edges.Where(edge => IsOnNonBlocking(edge, map, isBlocking)).ToList();

            var nodes = new List<InternalNode>();

            foreach (var node in walkableEdges)
            {
                if (node.Start == node.End)
                {
                    continue;
                }

                var startNode = nodes.Find(x => x.Point == node.Start);
                if (startNode == null)
                {
                    startNode = CreateNode(wallsTree, node.Start);
                    nodes.Add(startNode);
                }

                var endNode = nodes.Find(x => x.Point == node.End);
                if (endNode == null)
                {
                    endNode = CreateNode(wallsTree, node.End);
                    nodes.Add(endNode);
                }

                startNode.Neighbors.Add(endNode);
                endNode.Neighbors.Add(startNode);
            }

            PruneNodes(nodes, pruneDistanceToObstacleSquared);

            GetPointsOfInterest(nodes);

            var mergedNodes = MergeNodes(nodes, maxDistanceToMergeSquared);

            return MapToTerrainNodes(mergedNodes);
        }

        private static TerrainNode MapToTerrainNode(InternalNode internalNode, IDictionary<InternalNode, TerrainNode> mappedNodes)
        {
            if (mappedNodes.TryGetValue(internalNode, out var terrainNode))
            {
                return terrainNode;
            }

            var neighbors = new List<TerrainNode>();
            terrainNode = new TerrainNode(internalNode.Point, internalNode.IsChoke, neighbors);

            mappedNodes[internalNode] = terrainNode;

            neighbors.AddRange(internalNode.Neighbors.Select(x => MapToTerrainNode(x, mappedNodes)));

            return terrainNode;
        }

        private static IReadOnlyList<TerrainNode> MapToTerrainNodes(List<InternalNode> nodes)
        {
            var mappedNodes = new Dictionary<InternalNode, TerrainNode>();

            var terrainNodes = new List<TerrainNode>();
            foreach (var node in nodes)
            {
                if (mappedNodes.TryGetValue(node, out var terrainNode))
                {
                    terrainNodes.Add(terrainNode);
                    continue;
                }

                terrainNodes.Add(MapToTerrainNode(node, mappedNodes));
            }

            return terrainNodes;
        }

        private static List<InternalNode> MergeNodes(List<InternalNode> nodes, int maxDistanceToMergeSquared)
        {
            foreach (var node in nodes)
            {
                var removeCandidates = new Queue<InternalNode>(node.Neighbors);
                while (removeCandidates.Any())
                {
                    var removeCandidate = removeCandidates.Dequeue();
                    if (removeCandidate.Neighbors.Count < 2)
                    {
                        continue;
                    }

                    if (removeCandidate.IsChoke != node.IsChoke)
                    {
                        continue;
                    }

                    if (removeCandidate.Neighbors.Count > 2)
                    {
                        var distanceToCandidate = Point2D.DistanceSquared(node.Point, removeCandidate.Point);
                        if (distanceToCandidate > maxDistanceToMergeSquared)
                        {
                            continue;
                        }

                        var distanceSum = node.MinDistanceToObstacleSquared + removeCandidate.MinDistanceToObstacleSquared;
                        var x = (node.Point.X * node.MinDistanceToObstacleSquared + removeCandidate.Point.X * removeCandidate.MinDistanceToObstacleSquared) / distanceSum;
                        var y = (node.Point.Y * node.MinDistanceToObstacleSquared + removeCandidate.Point.Y * removeCandidate.MinDistanceToObstacleSquared) / distanceSum;

                        node.Point = new Point2D((int)x, (int)y);
                    }

                    foreach (var neighbor in removeCandidate.Neighbors)
                    {
                        neighbor.Neighbors.Remove(removeCandidate);
                        if (neighbor == node)
                        {
                            continue;
                        }

                        neighbor.Neighbors.Add(node);
                        node.Neighbors.Add(neighbor);
                        removeCandidates.Enqueue(neighbor);
                    }

                    removeCandidate.Neighbors.Clear();
                }
            }

            return nodes.Where(x => x.Neighbors.Any()).ToList();
        }

        private static void PruneNodes(ICollection<InternalNode> nodes, int pruneDistanceToObstacleSquared)
        {
            var leafVertices = new Queue<InternalNode>(nodes.Where(x => x.Neighbors.Count == 1));
            while (leafVertices.Any())
            {
                var node = leafVertices.Dequeue();

                if (node.Neighbors.Count != 1)
                {
                    continue;
                }

                var neighbor = node.Neighbors.Single();

                if (node.MinDistanceToObstacleSquared < pruneDistanceToObstacleSquared || node.MinDistanceToObstacleSquared - 0.9 <= neighbor.MinDistanceToObstacleSquared)
                {
                    nodes.Remove(node);
                    foreach (var n in node.Neighbors)
                    {
                        n.Neighbors.Remove(node);
                    }

                    if (neighbor.Neighbors.Count == 1)
                    {
                        leafVertices.Enqueue(neighbor);
                    }
                }
            }
        }

        private static void GetPointsOfInterest(IEnumerable<InternalNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Neighbors.Count != 2)
                {
                    node.IsChoke = false;

                    continue;
                }

                var neighborsMaxRadius = node.Neighbors.Max(x => x.MinDistanceToObstacleSquared);
                var neighborsMinRadius = node.Neighbors.Min(x => x.MinDistanceToObstacleSquared);

                if (node.MinDistanceToObstacleSquared < neighborsMinRadius)
                {
                    node.IsChoke = true;

                    continue;
                }

                if (node.MinDistanceToObstacleSquared > neighborsMaxRadius)
                {
                    node.IsChoke = false;
                }
            }
        }

        private static InternalNode CreateNode(RTree<Edge> wallsTree, Point2D point)
        {
            var closestObstacle = wallsTree.GetKNearest(1, point, (edge, p) => Math.Sqrt(DistanceSquared(p, edge.Start, edge.End))).First().Data;

            return new InternalNode
            {
                Point = point,
                Neighbors = new List<InternalNode>(),
                MinDistanceToObstacleSquared = DistanceSquared(point, closestObstacle.Start, closestObstacle.End)
            };
        }

        private static bool IsOnNonBlocking<TNode>(Edge edge, Map2D<TNode> map, Func<TNode, bool> isBlocking)
        {
            if (edge.Start.X < 0 || edge.Start.X >= map.Width || edge.Start.Y < 0 || edge.Start.Y >= map.Height)
            {
                return false;
            }

            if (edge.End.X < 0 || edge.End.X >= map.Width || edge.End.Y < 0 || edge.End.Y >= map.Height)
            {
                return false;
            }

            return !isBlocking(map[(int)edge.Start.X, (int)edge.Start.Y]) && !isBlocking(map[(int)edge.End.X, (int)edge.End.Y]);
        }

        private static double DistanceSquared(Point2D point, Point2D lineStart, Point2D lineEnd)
        {
            var segmentLength = Point2D.DistanceSquared(lineStart, lineEnd);
            if (Math.Abs(segmentLength) < 0.0001)
            {
                return Point2D.DistanceSquared(point, lineStart);
            }

            var t = ((point.X - lineStart.X) * (lineEnd.X - lineStart.X) + (point.Y - lineStart.Y) * (lineEnd.Y - lineStart.Y)) / segmentLength;
            t = Math.Max(0, Math.Min(1, t));

            var componentX = point.X - (lineStart.X + t * (lineEnd.X - lineStart.X));
            var componentY = point.Y - (lineStart.Y + t * (lineEnd.Y - lineStart.Y));

            return componentX * componentX + componentY * componentY;
        }

        private static void AddVerticalBorder(BoostVoronoi voronoi, RTree<Edge> wallsTree, IReadOnlyList<int> border, int x, int maxY)
        {
            var index = 0;
            if (!border.Any())
            {
                voronoi.AddSegment(x, 0, x, maxY);
                wallsTree.Insert(new Edge { Start = new Point2D(x, 0), End = new Point2D(x, maxY) }, new RTree<Edge>.Envelope(x, 0, x, maxY));
                return;
            }

            var firstY = border[index++];
            if (firstY != 0)
            {
                var y2 = index >= border.Count ? maxY : border[index++];
                voronoi.AddSegment(x, firstY, x, y2);
                wallsTree.Insert(new Edge { Start = new Point2D(x, firstY), End = new Point2D(x, y2) }, new RTree<Edge>.Envelope(x, firstY, x, y2));
            }

            while (index < border.Count)
            {
                var y1 = border[index++];
                var y2 = index >= border.Count ? maxY : border[index++];
                voronoi.AddSegment(x, y1, x, y2);
                wallsTree.Insert(new Edge { Start = new Point2D(x, y1), End = new Point2D(x, y2) }, new RTree<Edge>.Envelope(x, y1, x, y2));
            }
        }

        private static void AddHorizontalBorder(BoostVoronoi voronoi, RTree<Edge> wallsTree, IReadOnlyList<int> border, int y, int maxX)
        {
            var index = 0;
            if (!border.Any())
            {
                voronoi.AddSegment(0, y, maxX, y);
                wallsTree.Insert(new Edge { Start = new Point2D(0, y), End = new Point2D(maxX, y) }, new RTree<Edge>.Envelope(0, y, maxX, y));
                return;
            }

            var firstX = border[index++];
            if (firstX != 0)
            {
                var x2 = index >= border.Count ? maxX : border[index++];
                voronoi.AddSegment(firstX, y, x2, y);
                wallsTree.Insert(new Edge { Start = new Point2D(firstX, y), End = new Point2D(x2, y) }, new RTree<Edge>.Envelope(firstX, y, x2, y));
            }

            while (index < border.Count)
            {
                var x1 = border[index++];
                var x2 = index >= border.Count ? maxX : border[index++];
                voronoi.AddSegment(x1, y, x2, y);
                wallsTree.Insert(new Edge { Start = new Point2D(x1, y), End = new Point2D(x2, y) }, new RTree<Edge>.Envelope(x1, y, x2, y));
            }
        }
    }
}
