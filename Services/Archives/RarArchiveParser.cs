using SharpCompress.Archives.Rar;
using System;
using System.Linq;
using System.Text;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Interfaces;

namespace YWB.AntidetectAccountParser.Helpers
{
    public class RarArchiveParser:AbstractArchiveParser
    {
        public override void Parse(FacebookAccount fa,string filePath)
        {
            using (var archive = RarArchive.Open(filePath))
            {
                foreach (var entry in archive.Entries.Where(e=>!e.IsDirectory))
                {
                    if (entry.Key.ToLowerInvariant().Contains("password"))
                    {
                        Console.WriteLine($"Found file with passwords: {entry.Key}");
                        using (var s = entry.OpenEntryStream())
                        {
                            ExtractLoginAndPassword(fa, s);
                        }
                    }

                    if (entry.Key.ToLowerInvariant().Contains("cookie"))
                    {
                        Console.WriteLine($"Found file with cookies: {entry.Key}");
                        using (var s = entry.OpenEntryStream())
                        {
                            ExtractCookies(fa, s);
                        }
                    }
                }
            }
        }

    }
}
