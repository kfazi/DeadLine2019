namespace DeadLine2019.Algorithms
{
    public interface IMap2D<TNode>
    {
        TNode SafeNodeAt(int x, int y);

        TNode NodeAt(int x, int y);

        void SetNode(int x, int y, TNode node);

        TNode this[int x, int y] { get; set; }
    }
}