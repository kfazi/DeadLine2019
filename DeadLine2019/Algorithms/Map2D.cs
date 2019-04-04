namespace DeadLine2019.Algorithms
{
    using System.Collections.Generic;

    public class Map2D<TNode> where TNode : new()
    {
        private readonly TNode[] _nodes;

        public Map2D(int width, int height)
        {
            Width = width;
            Height = height;

            _nodes = new TNode[width * height];
            for (var i = 0; i < Nodes.Count; i++)
            {
                _nodes[i] = new TNode();
            }
        }

        public IReadOnlyList<TNode> Nodes => _nodes;

        public int Width { get; }

        public int Height { get; }

        public TNode NodeAt(int x, int y)
        {
            return Nodes[y * Width + x];
        }
    }
}