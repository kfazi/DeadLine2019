namespace DeadLine2019.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Priority_Queue;

    public static class AStar<TNode>
    {
        private class PriorityNode : FastPriorityQueueNode
        {
            public PriorityNode(TNode node, float heuristicCost)
                : this(null, node, 0, heuristicCost)
            {
            }

            public PriorityNode(PriorityNode parent, TNode node, float realCost, float heuristicCost)
            {
                Parent = parent;
                Node = node;
                Priority = realCost + heuristicCost;
                RealCost = realCost;
            }

            public PriorityNode Parent { get; }

            public TNode Node { get; }

            public float RealCost { get; }
        }

        private class PriorityNodeEqualityComparer : IEqualityComparer<PriorityNode>
        {
            private readonly IEqualityComparer<TNode> _equalityComparer;

            public PriorityNodeEqualityComparer(IEqualityComparer<TNode> equalityComparer)
            {
                _equalityComparer = equalityComparer;
            }

            public bool Equals(PriorityNode x, PriorityNode y)
            {
                return _equalityComparer.Equals(x.Node, y.Node);
            }

            public int GetHashCode(PriorityNode obj)
            {
                return _equalityComparer.GetHashCode(obj.Node);
            }
        }

        public static IEnumerable<TNode> FindPath(
            TNode start,
            TNode end,
            Func<TNode, IEnumerable<TNode>> getNeighbors,
            Func<TNode, TNode, float> getCost,
            Func<TNode, TNode, float> getHeuristicCost,
            int maxNodes = 1024)
        {
            return FindPath(start, end, getNeighbors, getCost, getHeuristicCost, EqualityComparer<TNode>.Default, maxNodes);
        }

        public static IEnumerable<TNode> FindPath(
            TNode start,
            TNode end,
            Func<TNode, IEnumerable<TNode>> getNeighbors,
            Func<TNode, TNode, float> getCost,
            Func<TNode, TNode, float> getHeuristicCost,
            IEqualityComparer<TNode> equalityComparer,
            int maxNodes = 1024)
        {
            var openList = new FastPriorityQueue<PriorityNode>(maxNodes);
            var closedList = new HashSet<PriorityNode>(new PriorityNodeEqualityComparer(equalityComparer));

            var startNode = new PriorityNode(start, getHeuristicCost(start, end));
            openList.Enqueue(startNode, startNode.Priority);

            while (openList.Any())
            {
                var currentNode = openList.Dequeue();

                if (equalityComparer.Equals(currentNode.Node, end))
                {
                    var path = new List<TNode>();

                    while (currentNode.Parent != null)
                    {
                        path.Add(currentNode.Node);
                        currentNode = currentNode.Parent;
                    }

                    return ((IEnumerable<TNode>)path).Reverse();
                }

                closedList.Add(currentNode);
                foreach (var neighbor in getNeighbors(currentNode.Node))
                {
                    var cost = currentNode.RealCost + getCost(currentNode.Node, neighbor);
                    if (float.IsInfinity(cost))
                    {
                        continue;
                    }

                    var openListNeighbor = openList.FirstOrDefault(x => equalityComparer.Equals(x.Node, neighbor));
                    if (openListNeighbor != null && cost < openListNeighbor.RealCost)
                    {
                        openList.Remove(openListNeighbor);
                        openListNeighbor = null;
                    }

                    var closedListNeighbor = closedList.FirstOrDefault(x => equalityComparer.Equals(x.Node, neighbor));
                    if (closedListNeighbor != null && cost < closedListNeighbor.RealCost)
                    {
                        closedList.Remove(closedListNeighbor);
                        closedListNeighbor = null;
                    }

                    if (openListNeighbor == null && closedListNeighbor == null)
                    {
                        if (openList.Count == openList.MaxSize)
                        {
                            continue;
                        }

                        var realCost = currentNode.RealCost + cost;
                        var heuristicCost = getHeuristicCost(neighbor, end);
                        var newNode = new PriorityNode(currentNode, neighbor, realCost, heuristicCost);
                        openList.Enqueue(newNode, newNode.Priority);
                    }
                }
            }

            return new List<TNode>();
        }
    }
}