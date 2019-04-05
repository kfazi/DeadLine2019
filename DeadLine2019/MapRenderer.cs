namespace DeadLine2019
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media.Imaging;

    using DeadLine2019.Algorithms;
    using DeadLine2019.Infrastructure;

    public class MapRenderer
    {
        public delegate IEnumerable<WriteableBitmap> ImageProvider<in TNode>(TNode node, int x, int y);

        private readonly Rect _sourceRect;

        private readonly int _cellWidth;

        private readonly int _cellHeight;

        private readonly double _minZoom;

        public MapRenderer(int cellWidth, int cellHeight)
        {
            _cellWidth = cellWidth;
            _cellHeight = cellHeight;
            _sourceRect = new Rect(0, 0, cellWidth, cellHeight);

            var smallestSize = Math.Min(cellWidth, cellHeight);
            _minZoom = 3.0 / smallestSize;
        }

        public void Render<TNode>(IMap2D<TNode> map, WriteableBitmap target, ImageProvider<TNode> imageProvider, DrawingWindowState drawingWindowState)
        {
            if (drawingWindowState.Z >= 1200 - _minZoom * 1200.0)
            {
                drawingWindowState.Z = (int)(1200 - _minZoom * 1200.0);
            }

            if (drawingWindowState.Z < 0)
            {
                drawingWindowState.Z = 0;
            }

            var scale = (1200 - drawingWindowState.Z) / 1200.0;
            var xOffset = drawingWindowState.X / (_cellWidth * scale);
            var yOffset = drawingWindowState.Y / (_cellHeight * scale);

            var cellWidth = (int)(_cellWidth * scale);
            var cellHeight = (int)(_cellHeight * scale);

            var xCellsAmount = target.PixelWidth / cellWidth;
            var yCellsAmount = target.PixelHeight / cellHeight;

            var xPixelOffset = (int)((xOffset - Math.Truncate(xOffset)) * cellWidth);
            var yPixelOffset = (int)((yOffset - Math.Truncate(yOffset)) * cellHeight);

            var targetRect = new Rect(0, 0, cellWidth, cellHeight);
            for (var y = 0; y <= yCellsAmount; y++)
            {
                for (var x = 0; x <= xCellsAmount; x++)
                {
                    var node = map.SafeNodeAt(x - (int)xOffset, y - (int)yOffset);
                    if (ReferenceEquals(node, null))
                    {
                        continue;
                    }

                    targetRect.X = x * cellWidth + xPixelOffset;
                    targetRect.Y = y * cellHeight + yPixelOffset;
                    foreach (var image in imageProvider(node, x, y))
                    {
                        target.Blit(targetRect, image, _sourceRect);
                    }
                }
            }
        }
    }
}