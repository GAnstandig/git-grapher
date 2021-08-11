using System;
using System.Collections.Generic;
using System.Linq;

namespace PrettyGit
{
    public static class Utilities
    {
        /// <summary>
        /// Searches for shortest path (using Dijkstra's Algorithm) between <paramref name="origin"/> and any <paramref name="destinations"/> if one exists
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destinations"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static List<Point>? GetShortestPath(Point origin, IEnumerable<Point> destinations, IEnumerable<Point> points)
        {
            HashSet<Point> queue = new(points);
            HashSet<Point> dests = new(destinations);

            Dictionary<Point, long> distance = new(points.Select(x => new KeyValuePair<Point, long>(x, long.MaxValue)));
            Dictionary<Point, Point?> previous = new(points.Select(x => new KeyValuePair<Point, Point?>(x, null)));

            distance[origin] = 0;

            bool found = false;
            while (queue.Any())
            {
                (Point pt, long dist ) = distance.OrderBy(x => x.Value).First(x => queue.Contains(x.Key));

                if (dist == long.MaxValue) 
                {
                    break;
                }

                queue.Remove(pt);
                if (destinations.Contains(pt)) 
                {
                    found = true;
                    break;
                }

                foreach (Point child in pt.Children)
                {
                    long altDistance = distance[pt] + 1 < 0 ? long.MaxValue : distance[pt] + 1;
                    
                    if (altDistance < distance[child])
                    {
                        distance[child] = altDistance;
                        previous[child] = pt;
                    }
                }
            }

            List<Point>? shortestPath = null;

            if (found) 
            {
                shortestPath = new();
                Point? position = previous.First(x => x.Value is not null && destinations.Contains(x.Key)).Key;

                while (position is not null)
                {
                    shortestPath.Add(position);
                    position = previous[position];
                }

                shortestPath.Reverse();
            }

            return shortestPath;
        }
    }
}
