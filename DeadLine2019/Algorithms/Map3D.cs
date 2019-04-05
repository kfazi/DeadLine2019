namespace DeadLine2019.Algorithms
{
    using System.Collections.Generic;

    public class Map3D<TNode> : IMap3D<TNode>
    {
        private readonly TNode[] _nodes;

        public Map3D(int width, int height, int depth)
        {
            Width = width;
            Height = height;
            Depth = depth;

            _nodes = new TNode[width * height * depth];
        }

        public IReadOnlyList<TNode> Nodes => _nodes;

        public int Width { get; }

        public int Height { get; }

        public int Depth { get; }

        public TNode SafeNodeAt(int x, int y, int z)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height || z < 0 || z >= Depth)
            {
                return default(TNode);
            }

            return _nodes[z * Width * Height + y * Width + x];
        }

        public TNode NodeAt(int x, int y, int z)
        {
            return _nodes[z * Width * Height + y * Width + x];
        }

        public void SetNode(int x, int y, int z, TNode node)
        {
            _nodes[z * Width * Height + y * Width + x] = node;
        }

        public TNode this[int x, int y, int z]
        {
            get => NodeAt(x, y, z);
            set => SetNode(x, y, z, value);
        }
    }
}