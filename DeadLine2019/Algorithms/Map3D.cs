namespace DeadLine2019.Algorithms
{
    using System.Collections.Generic;

    public class Map3D<TNode> where TNode : new()
    {
        private readonly TNode[] _nodes;

        public Map3D(int width, int height, int depth)
        {
            Width = width;
            Height = height;
            Depth = depth;

            _nodes = new TNode[width * height * depth];
            for (var i = 0; i < Nodes.Count; i++)
            {
                _nodes[i] = new TNode();
            }
        }

        public IReadOnlyList<TNode> Nodes => _nodes;

        public int Width { get; }

        public int Height { get; }

        public int Depth { get; }

        public TNode NodeAt(int x, int y, int z)
        {
            return Nodes[z * Width * Height + y * Width + x];
        }
    }
}