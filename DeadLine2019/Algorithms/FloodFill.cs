namespace DeadLine2019.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class FloodFill<TNode>
    {
        public static IEnumerable<TNode> GetNodes(
            TNode start,
            Func<TNode, IEnumerable<TNode>> getNeighbors,
            Func<TNode, bool> getAccessibility)
        {
            return GetNodes(start, getNeighbors, getAccessibility, EqualityComparer<TNode>.Default);
        }

        public static IEnumerable<TNode> GetNodes(
            TNode start,
            Func<TNode, IEnumerable<TNode>> getNeighbors,
            Func<TNode, bool> getAccessibility,
            IEqualityComparer<TNode> equalityComparer)
        {
            var result = new HashSet<TNode>(equalityComparer);

            if (!getAccessibility(start))
            {
                return result;
            }

            var nodesQueue = new Queue<TNode>();
            nodesQueue.Enqueue(start);

            while (nodesQueue.Any())
            {
                var node = nodesQueue.Dequeue();
                result.Add(node);

                foreach (var neighbor in getNeighbors(node))
                {
                    if (result.Contains(neighbor) || !getAccessibility(neighbor))
                    {
                        continue;
                    }

                    nodesQueue.Enqueue(neighbor);
                }
            }

            return result;
        }

        public static void FillNodes(
            TNode start,
            Func<TNode, IEnumerable<TNode>> getNeighbors,
            Func<TNode, bool> getAccessibility,
            Action<TNode> fillAction)
        {
            FillNodes(start, getNeighbors, getAccessibility, fillAction, EqualityComparer<TNode>.Default);
        }

        public static void FillNodes(
            TNode start,
            Func<TNode, IEnumerable<TNode>> getNeighbors,
            Func<TNode, bool> getAccessibility,
            Action<TNode> fillAction,
            IEqualityComparer<TNode> equalityComparer)
        {
            if (!getAccessibility(start))
            {
                return;
            }

            var visitedNodes = new HashSet<TNode>(equalityComparer);

            var nodesQueue = new Queue<TNode>();
            nodesQueue.Enqueue(start);

            while (nodesQueue.Any())
            {
                var node = nodesQueue.Dequeue();
                visitedNodes.Add(node);
                fillAction(node);

                foreach (var neighbor in getNeighbors(node))
                {
                    if (visitedNodes.Contains(neighbor) || !getAccessibility(neighbor))
                    {
                        continue;
                    }

                    nodesQueue.Enqueue(neighbor);
                }
            }
        }
    }
}