namespace DeadLine2019.Algorithms
{
    using System;
    using System.Collections.Generic;

    public class ComponentLabeling
    {
        public class Result
        {
            public Result(IReadOnlyCollection<Blob> blobs, Map2D<int> labels)
            {
                Blobs = blobs;
                Labels = labels;
            }

            public IReadOnlyCollection<Blob> Blobs { get; }

            public Map2D<int> Labels { get; }
        }

        public class Blob
        {
            private readonly List<IReadOnlyList<Point2D>> _internalContours = new List<IReadOnlyList<Point2D>>();

            public Blob(int label, IReadOnlyList<Point2D> externalContour)
            {
                Label = label;
                ExternalContour = externalContour;
            }

            public int Label { get; }

            public IReadOnlyList<Point2D> ExternalContour { get; }

            public IReadOnlyList<IReadOnlyList<Point2D>> InternalContours => _internalContours;

            public void AddInternalContour(IReadOnlyList<Point2D> contour)
            {
                _internalContours.Add(contour);
            }
        }

        public const int BackgroundType = 0;

        private static readonly Point2D[] DifferencePoints = {
            new Point2D(1, 0),
            new Point2D(1, 1),
            new Point2D(0, 1),
            new Point2D(-1, 1),
            new Point2D(-1, 0),
            new Point2D(-1, -1),
            new Point2D(0, -1),
            new Point2D(1, -1)
        };

        public static Result Trace<TNode>(Map2D<TNode> map, Func<TNode, int> getType)
        {
            var blobs = new List<Blob>();
            var labels = new Map2D<int>(map.Width, map.Height);

            var currentLabel = 1;

            for (var y = 0; y < map.Height; y++)
            {
                for (var x = 0; x < map.Width; x++)
                {
                    if (getType(map[x, y]) == BackgroundType)
                    {
                        continue;
                    }

                    var aboveIn = y > 0 ? getType(map[x, y - 1]) : BackgroundType;
                    var belowIn = y < map.Height - 1 ? getType(map[x, y + 1]) : BackgroundType;
                    var belowLabel = y < map.Height - 1 ? labels[x, y + 1] : -1;

                    if (labels[x, y] == 0 && aboveIn == BackgroundType)
                    {
                        var contour = TraceContour(labels, currentLabel, map, getType, x, y, true);

                        var blob = new Blob(currentLabel, contour);
                        blobs.Add(blob);

                        currentLabel++;
                    }
                    else if (belowIn == BackgroundType && belowLabel == 0)
                    {
                        var label = labels[x, y] != 0 ? labels[x, y] : labels[x - 1, y];

                        var contour = TraceContour(labels, label, map, getType, x, y, false);

                        var blob = blobs[label - 1];
                        blob.AddInternalContour(contour);
                    }
                    else if (labels[x, y] == 0)
                    {
                        labels[x, y] = x > 0 ? labels[x - 1, y] : 0;
                    }
                }
            }

            return new Result(blobs, labels);
        }

        private static List<Point2D> TraceContour<TNode>(IMap2D<int> labels, int currentLabel, Map2D<TNode> map, Func<TNode, int> getType, int x, int y, bool isExternal)
        {
            var contour = new List<Point2D>();

            var index = isExternal ? 7 : 3;

            var startX = x;
            var startY = y;

            var localX = -1;
            var localY = -1;

            labels[startX, startY] = currentLabel;

            var done = false;
            while (!done)
            {
                contour.Add(new Point2D(startX, startY));

                int counter;
                for (counter = 0; counter < 8; counter++, index = (index + 1) % 8)
                {
                    var differencePoint = DifferencePoints[index];
                    var currentX = (int)(startX + differencePoint.X);
                    var currentY = (int)(startY + differencePoint.Y);

                    if (currentX < 0 || currentX >= map.Width)
                    {
                        continue;
                    }

                    if (currentY < 0 || currentY >= map.Height)
                    {
                        continue;
                    }

                    if (getType(map[currentX, currentY]) != BackgroundType)
                    {
                        labels[currentX, currentY] = currentLabel;
                        if (localX < 0 && localY < 0)
                        {
                            localX = currentX;
                            localY = currentY;
                        }
                        else
                        {
                            done = x == startX && localX == currentX && y == startY && localY == currentY;
                        }

                        startX = currentX;
                        startY = currentY;
                        break;
                    }

                    labels[currentX, currentY] = -1;
                }

                if (counter == 8)
                {
                    done = true;
                }

                var previous = (index + 4) % 8;
                index = (previous + 2) % 8;
            }

            return contour;
        }
    }
}