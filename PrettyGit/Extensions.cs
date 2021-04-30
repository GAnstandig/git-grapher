using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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

        public static Range? GetBaseRange(this List<Range> collection, Range target)
        {
            Range? value = collection.Where
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
                .FirstOrDefault();

            return value;
        }

        public static byte ToByte(this string value)
        {
            return byte.Parse(value);
        }

        public static bool IsNumeric(this string value)
        {
            return !Regex.IsMatch(value, @"[a-zA-Z]");
        }
    }
}
