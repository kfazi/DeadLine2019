namespace DeadLine2019.Algorithms
{
    using System.Collections;
    using System.Collections.Generic;

    public class SortedCollection<T> : IReadOnlyList<T>, ICollection<T>
    {
        private readonly List<T> _list = new List<T>();

        private readonly IComparer<T> _comparer;

        public SortedCollection()
        {
            _comparer = Comparer<T>.Default;
        }

        public SortedCollection(IComparer<T> comparer)
        {
            _comparer = comparer;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _list.Add(item);
            _list.Sort(_comparer);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public bool IsReadOnly => false;

        public int Count => _list.Count;

        public T this[int index] => _list[index];
    }
}