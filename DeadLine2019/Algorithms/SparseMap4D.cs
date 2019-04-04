namespace DeadLine2019.Algorithms
{
    using System.Collections.Generic;

    public class SparseMap4D<TNode> where TNode : new()
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
                    return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
                }

                public int GetHashCode(SectorPosition obj)
                {
                    unchecked
                    {
                        var hashCode = obj.X;
                        hashCode = (hashCode * 397) ^ obj.Y;
                        hashCode = (hashCode * 397) ^ obj.Z;
                        hashCode = (hashCode * 397) ^ obj.W;
                        return hashCode;
                    }
                }
            }

            public static readonly IEqualityComparer<SectorPosition> Comparer = new EqualityComparer();

            public readonly int X;

            public readonly int Y;

            public readonly int Z;

            public readonly int W;

            public SectorPosition(int x, int y, int z, int w)
            {
                X = x;
                Y = y;
                Z = z;
                W = w;
            }
        }

        public delegate void OnSectorCreated(Map4D<TNode> sector, int xOffset, int yOffset, int zOffset, int wOffset);

        private readonly Dictionary<SectorPosition, Map4D<TNode>> _sectors = new Dictionary<SectorPosition, Map4D<TNode>>(SectorPosition.Comparer);

        private readonly int _sectorWidth;

        private readonly int _sectorHeight;

        private readonly int _sectorDepth;

        private readonly int _sectorFourthDimension;

        private readonly OnSectorCreated _onSectorCreated;

        public SparseMap4D(int sectorWidth, int sectorHeight, int sectorDepth, int sectorFourthDimension, OnSectorCreated onSectorCreated = null)
        {
            _sectorWidth = sectorWidth;
            _sectorHeight = sectorHeight;
            _sectorDepth = sectorDepth;
            _sectorFourthDimension = sectorFourthDimension;
            _onSectorCreated = onSectorCreated;
        }

        public Map4D<TNode> SectorAt(int x, int y, int z, int w)
        {
            var sectorPosition = new SectorPosition(x / _sectorWidth, y / _sectorHeight, z / _sectorDepth, w / _sectorFourthDimension);

            if (!_sectors.TryGetValue(sectorPosition, out var sector))
            {
                sector = new Map4D<TNode>(_sectorWidth, _sectorHeight, _sectorDepth, _sectorFourthDimension);
                _sectors[sectorPosition] = sector;
                _onSectorCreated?.Invoke(sector, sectorPosition.X * _sectorWidth, sectorPosition.Y * _sectorHeight, sectorPosition.Z * _sectorDepth, sectorPosition.W * _sectorFourthDimension);
            }

            return sector;
        }

        public TNode NodeAt(int x, int y, int z, int w)
        {
            var sector = SectorAt(x, y, z, w);
            return sector.NodeAt(x % _sectorWidth, y % _sectorHeight, z % _sectorDepth, w % _sectorFourthDimension);
        }
    }
}