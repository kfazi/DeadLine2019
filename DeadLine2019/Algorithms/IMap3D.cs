namespace DeadLine2019.Algorithms
{
    public interface IMap3D<TNode>
    {
        TNode SafeNodeAt(int x, int y, int z);

        TNode NodeAt(int x, int y, int z);

        void SetNode(int x, int y, int z, TNode node);

        TNode this[int x, int y, int z] { get; set; }
    }
}
