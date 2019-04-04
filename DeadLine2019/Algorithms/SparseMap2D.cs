namespace DeadLine2019.Algorithms
{
    using System.Collections.Generic;

    public class SparseMap2D<TNode> where TNode : new()
    {
        private class SectorPosition
        {
            private sealed class EqualityComparer : IEqualityComparer<SectorPosition>
            {
                public bool Equals(SectorPosition a, SectorPosition b)
                {
                    if (ReferenceEquals(a, b)) return true;
                    if (ReferenceEquals(a, null)) return false;
                    if (ReferenceEquals(b, null)) return false;
                    if (a.GetType() != b.GetType()) return false;
                    return a.X == b.X && a.Y == b.Y;
                }

                public int GetHashCode(SectorPosition obj)
                {
                    unchecked
                    {
                        return (obj.X * 397) ^ obj.Y;
                    }
                }
            }

            public static readonly IEqualityComparer<SectorPosition> Comparer = new EqualityComparer();

            public readonly int X;

            public readonly int Y;

            public SectorPosition(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public delegate void OnSectorCreated(Map2D<TNode> sector, int xOffset, int yOffset);

        private readonly Dictionary<SectorPosition, Map2D<TNode>> _sectors = new Dictionary<SectorPosition, Map2D<TNode>>(SectorPosition.Comparer);

        private readonly int _sectorWidth;

        private readonly int _sectorHeight;

        private readonly OnSectorCreated _onSectorCreated;

        public SparseMap2D(int sectorWidth, int sectorHeight, OnSectorCreated onSectorCreated = null)
        {
            _sectorWidth = sectorWidth;
            _sectorHeight = sectorHeight;
            _onSectorCreated = onSectorCreated;
        }

        public Map2D<TNode> SectorAt(int x, int y)
        {
            var sectorPosition = new SectorPosition(x / _sectorWidth, y / _sectorHeight);

            if (!_sectors.TryGetValue(sectorPosition, out var sector))
            {
                sector = new Map2D<TNode>(_sectorWidth, _sectorHeight);
                _sectors[sectorPosition] = sector;
                _onSectorCreated?.Invoke(sector, sectorPosition.X * _sectorWidth, sectorPosition.Y * _sectorHeight);
            }

            return sector;
        }

        public TNode NodeAt(int x, int y)
        {
            var sector = SectorAt(x, y);
            return sector.NodeAt(x % _sectorWidth, y % _sectorHeight);
        }
    }
}