namespace DeadLine2019.Algorithms
{
    using System.Collections.Generic;

    public class Map4D<TNode> : IMap4D<TNode>
    {
        private readonly TNode[] _nodes;

        public Map4D(int width, int height, int depth, int fourthDimension)
        {
            Width = width;
            Height = height;
            Depth = depth;
            FourthDimension = fourthDimension;

            _nodes = new TNode[width * height * depth * fourthDimension];
        }

        public IReadOnlyList<TNode> Nodes => _nodes;

        public int Width { get; }

        public int Height { get; }

        public int Depth { get; }

        public int FourthDimension { get; }

        public TNode SafeNodeAt(int x, int y, int z, int w)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height || z < 0 || z >= Depth || w < 0 || w >= FourthDimension)
            {
                return default(TNode);
            }

            return _nodes[w * Depth * Height * Width + z * Width * Height + y * Width + x];
        }

        public TNode NodeAt(int x, int y, int z, int w)
        {
            return _nodes[w * Depth * Height * Width + z * Width * Height + y * Width + x];
        }

        public void SetNode(int x, int y, int z, int w, TNode node)
        {
            _nodes[w * Depth * Height * Width + z * Width * Height + y * Width + x] = node;
        }

        public TNode this[int x, int y, int z, int w]
        {
            get => NodeAt(x, y, z, w);
            set => SetNode(x, y, z, w, value);
        }
    }
}