using System;
using System.IO.Compression;
using System.Linq;
using System.Text;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Interfaces;

namespace YWB.AntidetectAccountParser.Helpers
{
    public class ZipArchiveParser:AbstractArchiveParser
    {
        public override void Parse(FacebookAccount fa,string filePath)
        {
            using (var archive = ZipFile.OpenRead(filePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.ToLowerInvariant().Contains("password"))
                    {
                        Console.WriteLine($"Found file with passwords: {entry.FullName}");
                        using (var s = entry.Open())
                        {
                            ExtractLoginAndPassword(fa, s);
                        }
                    }

                    if (entry.FullName.ToLowerInvariant().Contains("cookie") && entry.Length > 0)
                    {
                        Console.WriteLine($"Found file with cookies: {entry.FullName}");
                        using (var s = entry.Open())
                        {
                            ExtractCookies(fa, s);
                        }
                    }

                    if (entry.FullName.ToLowerInvariant().Contains("token") && entry.Length > 0)
                    {
                        Console.WriteLine($"Found file with access token: {entry.FullName}");
                        using (var s = entry.Open())
                        {
                            ExtractToken(fa, s);
                        }
                    }
                }
            }
        }
    }
}
