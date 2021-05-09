using System.Collections.Generic;
using System.Linq;

namespace PrettyGit
{
    public static class Extensions
    {
        public static void RemoveRange<T>(this List<T> container, List<T> objects)
        {
            foreach (T obj in objects)
            {
                if (container.Contains(obj))
                {
                    container.Remove(obj);
                }
            }
        }

        public static List<Range>? GetIntersectingRanges(this List<Range> collection, Range target) 
        {
            List<Range>? intersectingRanges = collection.Where
                (x =>
                    (x.StartPoint <= target.StartPoint &&
                    x.EndPoint >= target.StartPoint)
                    || (x.StartPoint <= target.StartPoint &&
                    x.EndPoint >= target.StartPoint)
                    || (x.StartPoint <= target.EndPoint &&
                    x.EndPoint >= target.EndPoint)
                    || (x.StartPoint >= target.StartPoint &&
                    x.EndPoint <= target.EndPoint)
                )
                .OrderByDescending(x => x.YOffset)
                .ToList();

            return intersectingRanges;
        }

        public static Range? GetBaseRange(this List<Range> collection, Range target)
        {
            return collection.GetIntersectingRanges(target).FirstOrDefault();
        }

        public static byte ToByte(this string value)
        {
            return byte.Parse(value);
        }

    }
}
