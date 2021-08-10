using System;
using System.Collections.Generic;
using System.Linq;

namespace PrettyGit
{
    public static class Utilities
    {
        /// <summary>
        /// Searches for shortest path (using Dijkstra's Algorithm) between <paramref name="origin"/> and <paramref name="destination"/> if one exists
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static List<Point>? GetShortestPath(Point origin, Point destination, IEnumerable<Point> points)
        {
            HashSet<Point> queue = new(points);
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
                if (pt.Equals(destination)) 
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
                Point? position = destination;

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
