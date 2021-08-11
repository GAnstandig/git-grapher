using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
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
        public static List<Node>? GetShortestPath(Node origin, IEnumerable<Node> destinations, IEnumerable<Node> points)
        {
            HashSet<Node> queue = new(points);
            HashSet<Node> dests = new(destinations);

            Dictionary<Node, long> distance = new(points.Select(x => new KeyValuePair<Node, long>(x, long.MaxValue)));
            Dictionary<Node, Node?> previous = new(points.Select(x => new KeyValuePair<Node, Node?>(x, null)));

            distance[origin] = 0;

            bool found = false;
            while (queue.Any())
            {
                (Node pt, long dist ) = distance.OrderBy(x => x.Value).First(x => queue.Contains(x.Key));

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

                foreach (Node child in pt.Children)
                {
                    long altDistance = distance[pt] + 1 < 0 ? long.MaxValue : distance[pt] + 1;
                    
                    if (altDistance < distance[child])
                    {
                        distance[child] = altDistance;
                        previous[child] = pt;
                    }
                }
            }

            List<Node>? shortestPath = null;

            if (found) 
            {
                shortestPath = new();
                Node? position = previous.First(x => x.Value is not null && destinations.Contains(x.Key)).Key;

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
