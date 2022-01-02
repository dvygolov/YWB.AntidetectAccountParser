using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YWB.Helpers
{
    public class SelectHelper
    {
        public static T Select<T>(IEnumerable<T> items, Func<T, string> convert = null)
        {
            int i = 1;
            foreach (var item in items)
            {
                var str = convert != null ? convert(item) : item.ToString();
                Console.WriteLine($"{i}.{str}");
                i++;
            }
            Console.Write("Your choice:");
            var index = int.Parse(Console.ReadLine()) - 1;
            return Enumerable.ElementAt(items, index);
        }

        public static async Task<T> SelectWithCreateAsync<T>(IEnumerable<T> items, Func<T, string> convert = null, Func<Task<T>> create = null, bool addEmpty = false)
        {
            int i = 1;
            foreach (var item in items)
            {
                var str = convert != null ? convert(item) : item.ToString();
                Console.WriteLine($"{i}.{str}");
                i++;
            }
            if (addEmpty)
            {
                Console.WriteLine($"{i} Don't use");
                i++;
            }

            if (create != null)
            {
                Console.WriteLine($"{i} Create new!");
            }
            Console.Write("Your choice:");
            var index = int.Parse(Console.ReadLine()) - 1;
            var count = Enumerable.Count(items);
            if (index == count)
                return default(T);
            else if (index == count + 1)
                return await create();
            else
                return Enumerable.ElementAt(items, index);
        }

        public static List<T> SelectMultiple<T>(IEnumerable<T> items, Func<T, string> convert = null)
        {
            int i = 1;
            foreach (var item in items)
            {
                var str = convert != null ? convert(item) : item.ToString();
                Console.WriteLine($"{i}.{str}");
                i++;
            }
            Console.Write("Your choice (For example:1-10,11):");
            var indexesStr = Console.ReadLine();
            var split = indexesStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var indexes = new List<int>();
            foreach (var s in split)
            {
                if (s.Contains('-'))
                {
                    var start = int.Parse(s.Substring(0, s.IndexOf('-'))) - 1;
                    var end = int.Parse(s.Substring(s.IndexOf('-') + 1, s.Length - s.IndexOf('-') - 1)) - 1;
                    indexes.AddRange(Enumerable.Range(start, end - start + 1));
                }
                else
                    indexes.Add(int.Parse(s) - 1);
            }

            return indexes.Select(i => Enumerable.ElementAt(items, i)).ToList();
        }
    }
}
