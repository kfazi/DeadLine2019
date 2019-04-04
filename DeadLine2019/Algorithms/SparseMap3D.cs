namespace DeadLine2019.Algorithms
{
    using System.Collections.Generic;

    public class SparseMap3D<TNode> where TNode : new()
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
                    return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
                }

                public int GetHashCode(SectorPosition obj)
                {
                    unchecked
                    {
                        var hashCode = obj.X;
                        hashCode = (hashCode * 397) ^ obj.Y;
                        hashCode = (hashCode * 397) ^ obj.Z;
                        return hashCode;
                    }
                }
            }

            public static readonly IEqualityComparer<SectorPosition> Comparer = new EqualityComparer();

            public readonly int X;

            public readonly int Y;

            public readonly int Z;

            public SectorPosition(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        public delegate void OnSectorCreated(Map3D<TNode> sector, int xOffset, int yOffset, int zOffset);

        private readonly Dictionary<SectorPosition, Map3D<TNode>> _sectors = new Dictionary<SectorPosition, Map3D<TNode>>(SectorPosition.Comparer);

        private readonly int _sectorWidth;

        private readonly int _sectorHeight;

        private readonly int _sectorDepth;

        private readonly OnSectorCreated _onSectorCreated;

        public SparseMap3D(int sectorWidth, int sectorHeight, int sectorDepth, OnSectorCreated onSectorCreated = null)
        {
            _sectorWidth = sectorWidth;
            _sectorHeight = sectorHeight;
            _sectorDepth = sectorDepth;
            _onSectorCreated = onSectorCreated;
        }

        public Map3D<TNode> SectorAt(int x, int y, int z)
        {
            var sectorPosition = new SectorPosition(x / _sectorWidth, y / _sectorHeight, z / _sectorDepth);

            if (!_sectors.TryGetValue(sectorPosition, out var sector))
            {
                sector = new Map3D<TNode>(_sectorWidth, _sectorHeight, _sectorDepth);
                _sectors[sectorPosition] = sector;
                _onSectorCreated?.Invoke(sector, sectorPosition.X * _sectorWidth, sectorPosition.Y * _sectorHeight, sectorPosition.Z * _sectorDepth);
            }

            return sector;
        }

        public TNode NodeAt(int x, int y, int z)
        {
            var sector = SectorAt(x, y, z);
            return sector.NodeAt(x % _sectorWidth, y % _sectorHeight, z % _sectorDepth);
        }
    }
}