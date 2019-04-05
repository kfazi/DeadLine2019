namespace DeadLine2019.Algorithms
{
    public interface IMap4D<TNode>
    {
        TNode SafeNodeAt(int x, int y, int z, int w);

        TNode NodeAt(int x, int y, int z, int w);

        void SetNode(int x, int y, int z, int w, TNode node);

        TNode this[int x, int y, int z, int w] { get; set; }
    }
}