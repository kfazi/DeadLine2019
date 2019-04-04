namespace DeadLine2019.Algorithms
{
    using System.Collections.Generic;

    public class Map4D<TNode> where TNode : new()
    {
        private readonly TNode[] _nodes;

        public Map4D(int width, int height, int depth, int fourthDimension)
        {
            Width = width;
            Height = height;
            Depth = depth;
            FourthDimension = fourthDimension;

            _nodes = new TNode[width * height * depth * fourthDimension];
            for (var i = 0; i < Nodes.Count; i++)
            {
                _nodes[i] = new TNode();
            }
        }

        public IReadOnlyList<TNode> Nodes => _nodes;

        public int Width { get; }

        public int Height { get; }

        public int Depth { get; }

        public int FourthDimension { get; }

        public TNode NodeAt(int x, int y, int z, int w)
        {
            return Nodes[w * Depth * Height * Width + z * Width * Height + y * Width + x];
        }
    }
}