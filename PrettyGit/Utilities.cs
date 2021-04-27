using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrettyGit
{
    public static class Utilities
    {
        public static List<Point>? GetShortestPath(Point origin, Point destination, List<Point>? points = null)
        {
            if (points is null)
            {
                points = new List<Point>() { origin };
            }

            if (points.Contains(destination))
            {
                //backtrack from destination to find origin using values in points
                List<Point> path = new() { destination };
                Point next = destination;

                do
                {
                    next = next.Parents.Where(x => points.Contains(x)).First();
                    path.Add(next);
                } while (next != origin);

                return ((IEnumerable<Point>)path).Reverse().ToList();
            }
            else
            {
                HashSet<Point> children = new();

                foreach (Point point in points)
                {
                    point.Children.ForEach(x =>
                    {
                        if (!children.Contains(x))
                        {
                            children.Add(x);
                        }
                    });
                }

                if (children.Except(points).Any())
                //add all previously-unknown children to points list and recurse
                {
                    points.AddRange(children);
                    return GetShortestPath(origin, destination, points.Distinct().ToList());
                }
                else
                //no more children so no path exists.
                {
                    return null;
                }
            }
        }
    }
}
