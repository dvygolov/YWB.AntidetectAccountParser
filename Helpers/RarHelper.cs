using SharpCompress.Archives.Rar;
using System;
using System.IO.Compression;
using System.Linq;
using System.Text;
using YWB.AntidetectAccountParser.Model;

namespace YWB.AntidetectAccountParser.Helpers
{
    public class RarHelper
    {
        public static void Parse(FacebookAccount fa,string f)
        {
            using (var archive = RarArchive.Open(f))
            {
                foreach (var entry in archive.Entries.Where(e=>!e.IsDirectory))
                {
                    if (entry.Key.ToLowerInvariant().Contains("password"))
                    {
                        Console.WriteLine($"Found file with passwords: {entry.Key}");
                        using (var s = entry.OpenEntryStream())
                        {
                            var lines = Encoding.UTF8.GetString(s.ReadAllBytes()).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            int index = -1;
                            while ((index = lines.FindIndex(index + 1, l => l.ToLowerInvariant().Contains("facebook"))) != -1)
                            {
                                var login = lines[index + 1].Split(' ')[1];
                                var password = lines[index + 2].Split(' ')[1];
                                if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                                {
                                    if (fa.AddLoginPassword(login, password))
                                        Console.WriteLine("Found Facebook login/password!");
                                }
                            }
                        }
                    }

                    if (entry.Key.ToLowerInvariant().Contains("cookie"))
                    {
                        Console.WriteLine($"Found file with cookies: {entry.Key}");
                        using (var s = entry.OpenEntryStream())
                        {
                            var text = Encoding.UTF8.GetString(s.ReadAllBytes());
                            string cookies = string.Empty;
                            if (!text.Trim().StartsWith('['))
                            {
                                cookies = CookieHelper.NetscapeCookiesToJSON(text);
                            }
                            else
                                cookies = text;
                            var fbCookies = CookieHelper.GetFacebookCookies(cookies);
                            if (!string.IsNullOrEmpty(fbCookies))
                                if (fa.AddCookies(fbCookies))
                                    Console.WriteLine("Found Facebook cookies!");
                        }
                    }
                }
            }
        }
    }
}
