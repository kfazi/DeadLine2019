namespace DeadLine2019.Algorithms
{
    using System.Collections.Generic;
    using System.Linq;

    public static class DouglasPeucker
    {
        public delegate double Distance<in TPoint>(TPoint point, TPoint segmentStart, TPoint segmentEnd);

        public static List<TPoint> Simplify<TPoint>(IReadOnlyList<TPoint> points, double epsilon, Distance<TPoint> getDistance)
        {
            var maxDistance = 0.0;
            var splitIndex = 0;
            for (var index = 1; index < points.Count - 1; index++)
            {
                var distance = getDistance(points[index], points.First(), points.Last());
                if (distance <= maxDistance)
                {
                    continue;
                }

                maxDistance = distance;
                splitIndex = index;
            }

            if (maxDistance < epsilon)
            {
                return new List<TPoint>
                {
                    points.First(),
                    points.Last()
                };
            }

            var results1 = Simplify(points.Take(splitIndex).ToList(), epsilon, getDistance);
            var results2 = Simplify(points.Skip(splitIndex).ToList(), epsilon, getDistance);

            return results1.Take(results1.Count - 1).Concat(results2).ToList();
        }
    }
}