namespace DeadLine2019.Algorithms
{
    using System.Collections.Generic;

    public class Map2D<TNode> : IMap2D<TNode>
    {
        private readonly TNode[] _nodes;

        public Map2D(int width, int height)
        {
            Width = width;
            Height = height;

            _nodes = new TNode[width * height];
        }

        public IReadOnlyList<TNode> Nodes => _nodes;

        public int Width { get; }

        public int Height { get; }

        public TNode SafeNodeAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
            {
                return default(TNode);
            }

            return _nodes[y * Width + x];
        }

        public TNode NodeAt(int x, int y)
        {
            return _nodes[y * Width + x];
        }

        public void SetNode(int x, int y, TNode node)
        {
            _nodes[y * Width + x] = node;
        }

        public TNode this[int x, int y]
        {
            get => NodeAt(x, y);
            set => SetNode(x, y, value);
        }
    }
}