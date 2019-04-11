﻿namespace DeadLine2019.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using DeadLine2019.Infrastructure;

    public class RTree<TNode>
    {
        public class Envelope
        {
            public Envelope()
            {
            }

            public Envelope(int x1, int y1, int x2, int y2)
            {
                X1 = x1;
                Y1 = y1;
                X2 = x2;
                Y2 = y2;
            }

            public Envelope(Point2D a, Point2D b)
            {
                X1 = (int)Math.Min(a.X, b.X);
                Y1 = (int)Math.Min(a.Y, b.Y);
                X2 = (int)Math.Max(a.X, b.X);
                Y2 = (int)Math.Max(a.Y, b.Y);
            }

            public int X1 { get; private set; }

            public int Y1 { get; private set; }

            public int X2 { get; private set; }

            public int Y2 { get; private set; }

            public int Area => (X2 - X1) * (Y2 - Y1);

            public int Margin => X2 - X1 + (Y2 - Y1);

            public void Extend(Envelope by)
            {
                X1 = Math.Min(X1, by.X1);
                Y1 = Math.Min(Y1, by.Y1);
                X2 = Math.Max(X2, by.X2);
                Y2 = Math.Max(Y2, by.Y2);
            }

            public override string ToString()
            {
                return $"{X1},{Y1} - {X2},{Y2}";
            }

            public bool Intersects(Envelope b)
            {
                return b.X1 <= X2 && b.Y1 <= Y2 && b.X2 >= X1 && b.Y2 >= Y1;
            }

            public bool Contains(Envelope b)
            {
                return X1 <= b.X1 && Y1 <= b.Y1 && b.X2 <= X2 && b.Y2 <= Y2;
            }
        }

        public class Node
        {
            public Node() : this(default, new Envelope())
            {
            }

            public Node(TNode data, Envelope envelope)
            {
                Data = data;
                Envelope = envelope;
            }

            public TNode Data { get; }

            public Envelope Envelope { get; set; }

            public bool IsLeaf { get; set; }

            public int Height { get; set; }

            public List<Node> Children { get; } = new List<Node>();
        }

        private static readonly EqualityComparer<TNode> Comparer = EqualityComparer<TNode>.Default;

        private readonly int _maxEntries;

        private readonly int _minEntries;

        private Node _root;

        public RTree(int minEntries, int maxEntries)
        {
            _maxEntries = Math.Max(4, maxEntries);
            _minEntries = Math.Max(2, minEntries);

            Clear();
        }

        public void Load(IReadOnlyList<Node> nodes)
        {
            if (nodes.Count < _minEntries)
            {
                foreach (var n in nodes) Insert(n);

                return;
            }

            var node = BuildOneLevel(nodes.OrderBy(n => n.Envelope.X1).ToList(), 0, 0);

            if (_root.Children.Count == 0)
            {
                _root = node;

            }
            else if (_root.Height == node.Height)
            {
                SplitRoot(_root, node);
            }
            else
            {
                if (_root.Height < node.Height)
                {
                    var tmpNode = _root;
                    _root = node;
                    node = tmpNode;
                }

                Insert(node, _root.Height - node.Height - 1);
            }
        }

        private Node BuildOneLevel(IReadOnlyCollection<Node> items, int level, int height)
        {
            Node node;
            var n = items.Count;
            var m = _maxEntries;

            if (n <= m)
            {
                node = new Node { IsLeaf = true, Height = 1 };
                node.Children.AddRange(items);
            }
            else
            {
                if (level == 0)
                {
                    height = (int)Math.Ceiling(Math.Log(n) / Math.Log(m));

                    m = (int)Math.Ceiling(n / Math.Pow(m, height - 1));
                }

                node = new Node { Height = height };

                var n1 = (int)(Math.Ceiling((double)n / m) * Math.Ceiling(Math.Sqrt(m)));
                var n2 = (int)Math.Ceiling((double)n / m);

                var compare = level % 2 == 1
                                ? (Comparison<Node>)CompareNodesByMinX
                                : CompareNodesByMinY;

                for (var i = 0; i < n; i += n1)
                {
                    var slice = items.Skip(i).Take(n1).ToList();
                    slice.Sort(compare);

                    for (var j = 0; j < slice.Count; j += n2)
                    {
                        var childNode = BuildOneLevel(slice.GetRange(j, n2), level + 1, height - 1);
                        node.Children.Add(childNode);
                    }
                }
            }

            RefreshEnvelope(node);

            return node;
        }

        public IEnumerable<Node> Search(Envelope envelope)
        {
            var node = _root;

            if (!envelope.Intersects(node.Envelope))
            {
                return Enumerable.Empty<Node>();
            }

            var result = new List<Node>();
            var nodesToSearch = new Stack<Node>();

            while (node != null)
            {
                foreach (var child in node.Children)
                {
                    var childEnvelope = child.Envelope;

                    if (!envelope.Intersects(childEnvelope))
                    {
                        continue;
                    }

                    if (node.IsLeaf)
                    {
                        result.Add(child);
                    }
                    else if (envelope.Contains(childEnvelope))
                    {
                        Collect(child, result);
                    }
                    else
                    {
                        nodesToSearch.Push(child);
                    }
                }

                if (!nodesToSearch.TryPop(out node))
                {
                    break;
                }
            }

            return result;
        }

        private static void Collect(Node node, List<Node> result)
        {
            var nodesToSearch = new Stack<Node>();
            while (node != null)
            {
                if (node.IsLeaf)
                {
                    result.AddRange(node.Children);
                }
                else
                {
                    foreach (var n in node.Children)
                    {
                        nodesToSearch.Push(n);
                    }
                }

                if (!nodesToSearch.TryPop(out node))
                {
                    break;
                }
            }
        }

        public void Clear()
        {
            _root = new Node
            {
                IsLeaf = true,
                Height = 1
            };
        }

        public void Insert(Node item)
        {
            Insert(item, _root.Height - 1);
        }

        public void Insert(TNode data, Envelope bounds)
        {
            Insert(new Node(data, bounds));
        }

        private void Insert(Node item, int level)
        {
            var envelope = item.Envelope;
            var insertPath = new List<Node>();

            var node = ChooseSubtree(envelope, _root, level, insertPath);

            node.Children.Add(item);
            node.Envelope.Extend(envelope);

            while (level >= 0)
            {
                if (insertPath[level].Children.Count <= _maxEntries)
                {
                    break;
                }

                Split(insertPath, level);
                level--;
            }

            AdjustParentBounds(envelope, insertPath, level);
        }

        private static int CombinedArea(Envelope what, Envelope with)
        {
            var minX1 = Math.Max(what.X1, with.X1);
            var minY1 = Math.Max(what.Y1, with.Y1);
            var maxX2 = Math.Min(what.X2, with.X2);
            var maxY2 = Math.Min(what.Y2, with.Y2);

            return (maxX2 - minX1) * (maxY2 - minY1);
        }

        private static int IntersectionArea(Envelope what, Envelope with)
        {
            var minX = Math.Max(what.X1, with.X1);
            var minY = Math.Max(what.Y1, with.Y1);
            var maxX = Math.Min(what.X2, with.X2);
            var maxY = Math.Min(what.Y2, with.Y2);

            return Math.Max(0, maxX - minX) * Math.Max(0, maxY - minY);
        }

        private static Node ChooseSubtree(Envelope boundingBox, Node node, int level, ICollection<Node> path)
        {
            while (true)
            {
                path.Add(node);

                if (node.IsLeaf || path.Count - 1 == level)
                {
                    break;
                }

                var minArea = int.MaxValue;
                var minEnlargement = int.MaxValue;

                Node targetNode = null;

                foreach (var child in node.Children)
                {
                    var area = child.Envelope.Area;
                    var enlargement = CombinedArea(boundingBox, child.Envelope) - area;

                    if (enlargement < minEnlargement)
                    {
                        minEnlargement = enlargement;
                        minArea = area < minArea ? area : minArea;
                        targetNode = child;

                    }
                    else if (enlargement == minEnlargement)
                    {
                        if (area >= minArea)
                        {
                            continue;
                        }

                        minArea = area;
                        targetNode = child;
                    }
                }

                node = targetNode;
            }

            return node;
        }

        private void Split(IReadOnlyList<Node> insertPath, int level)
        {
            var node = insertPath[level];
            var totalCount = node.Children.Count;

            ChooseSplitAxis(node, _minEntries, totalCount);

            var newNode = new Node { Height = node.Height };
            var splitIndex = ChooseSplitIndex(node, _minEntries, totalCount);

            newNode.Children.AddRange(node.Children.GetRange(splitIndex, node.Children.Count - splitIndex));
            node.Children.RemoveRange(splitIndex, node.Children.Count - splitIndex);

            if (node.IsLeaf)
            {
                newNode.IsLeaf = true;
            }

            RefreshEnvelope(node);
            RefreshEnvelope(newNode);

            if (level > 0)
            {
                insertPath[level - 1].Children.Add(newNode);
            }
            else
            {
                SplitRoot(node, newNode);
            }
        }

        private void SplitRoot(Node node, Node newNode)
        {
            _root = new Node
            {
                Children = { node, newNode },
                Height = node.Height + 1
            };

            RefreshEnvelope(_root);
        }

        private static int ChooseSplitIndex(Node node, int minEntries, int totalCount)
        {
            var minOverlap = int.MaxValue;
            var minArea = int.MaxValue;
            var index = 0;

            for (var i = minEntries; i <= totalCount - minEntries; i++)
            {
                var boundingBox1 = SumChildBounds(node, 0, i);
                var boundingBox2 = SumChildBounds(node, i, totalCount);

                var overlap = IntersectionArea(boundingBox1, boundingBox2);
                var area = boundingBox1.Area + boundingBox2.Area;

                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    index = i;

                    minArea = area < minArea ? area : minArea;
                }
                else if (overlap == minOverlap)
                {
                    if (area >= minArea)
                    {
                        continue;
                    }

                    minArea = area;
                    index = i;
                }
            }

            return index;
        }

        public void Remove(TNode item, Envelope envelope)
        {
            var node = _root;
            var itemEnvelope = envelope;

            var path = new Stack<Node>();
            var indexes = new Stack<int>();

            var i = 0;
            var goingUp = false;
            Node parent = null;

            while (node != null || path.Count > 0)
            {
                if (node == null)
                {
                    if (!path.TryPop(out node))
                    {
                        node = null;
                    }

                    if (!path.TryPeek(out parent))
                    {
                        parent = null;
                    }

                    if (!indexes.TryPop(out i))
                    {
                        i = 0;
                    }

                    goingUp = true;
                }

                if (node != null && node.IsLeaf)
                {
                    var index = node.Children.FindIndex(n => Comparer.Equals(item, n.Data));

                    if (index != -1)
                    {
                        node.Children.RemoveAt(index);
                        path.Push(node);
                        CondenseNodes(path.ToArray());

                        return;
                    }
                }

                if (!goingUp && !node.IsLeaf && node.Envelope.Contains(itemEnvelope))
                {
                    path.Push(node);
                    indexes.Push(i);
                    i = 0;
                    parent = node;
                    node = node.Children[0];

                }
                else if (parent != null)
                {
                    i++;
                    if (i == parent.Children.Count)
                    {
                        node = null;
                    }
                    else
                    {
                        node = parent.Children[i];
                        goingUp = false;
                    }
                }
                else
                {
                    node = null;
                }
            }
        }

        private void CondenseNodes(IList<Node> path)
        {
            for (var i = path.Count - 1; i >= 0; i--)
            {
                if (path[i].Children.Count == 0)
                {
                    if (i == 0)
                    {
                        Clear();
                    }
                    else
                    {
                        var siblings = path[i - 1].Children;
                        siblings.Remove(path[i]);
                    }
                }
                else
                {
                    RefreshEnvelope(path[i]);
                }
            }
        }

        private static void RefreshEnvelope(Node node)
        {
            node.Envelope = SumChildBounds(node, 0, node.Children.Count);
        }

        private static Envelope SumChildBounds(Node node, int startIndex, int endIndex)
        {
            var envelope = new Envelope();

            for (var i = startIndex; i < endIndex; i++)
            {
                envelope.Extend(node.Children[i].Envelope);
            }

            return envelope;
        }

        private static void AdjustParentBounds(Envelope boundingBox, IReadOnlyList<Node> path, int level)
        {
            for (var i = level; i >= 0; i--)
            {
                path[i].Envelope.Extend(boundingBox);
            }
        }

        private static void ChooseSplitAxis(Node node, int min, int max)
        {
            var xMargin = AllDistMargin(node, min, max, CompareNodesByMinX);
            var yMargin = AllDistMargin(node, min, max, CompareNodesByMinY);

            if (xMargin < yMargin)
            {
                node.Children.Sort(CompareNodesByMinX);
            }
        }

        private static int CompareNodesByMinX(Node a, Node b)
        {
            return a.Envelope.X1.CompareTo(b.Envelope.X1);
        }

        private static int CompareNodesByMinY(Node a, Node b)
        {
            return a.Envelope.Y1.CompareTo(b.Envelope.Y1);
        }

        private static int AllDistMargin(Node node, int min, int max, Comparison<Node> compare)
        {
            node.Children.Sort(compare);

            var leftBBox = SumChildBounds(node, 0, min);
            var rightBBox = SumChildBounds(node, max - min, max);
            var margin = leftBBox.Margin + rightBBox.Margin;

            for (var i = min; i < max - min; i++)
            {
                var child = node.Children[i];
                leftBBox.Extend(child.Envelope);
                margin += leftBBox.Margin;
            }

            for (var i = max - min - 1; i >= min; i--)
            {
                var child = node.Children[i];
                rightBBox.Extend(child.Envelope);
                margin += rightBBox.Margin;
            }

            return margin;
        }
    }
}