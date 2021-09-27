using System.Collections.Generic;

namespace YWB.AntidetectAccountParser.Helpers
{
    public static class RandomHelper
    {
        public static T GetRandomEntryFromList<T>(this List<T> entries)
        {
            var r = StaticRandom.Instance;
            var lineNumber = r.Next(0, entries.Count);
            return entries[lineNumber];
        }
    }
}
