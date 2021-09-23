using System;
using System.IO.Compression;
using System.Linq;
using System.Text;
using YWB.AntidetectAccountParser.Model;

namespace YWB.AntidetectAccountParser.Helpers
{
    public class ZipHelper
    {
        public static void Parse(FacebookAccount fa,string f)
        {
            using (var archive = ZipFile.OpenRead(f))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.ToLowerInvariant().Contains("password"))
                    {
                        Console.WriteLine($"Found file with passwords: {entry.FullName}");
                        using (var s = entry.Open())
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

                    if (entry.FullName.ToLowerInvariant().Contains("cookie") && entry.Length > 0)
                    {
                        Console.WriteLine($"Found file with cookies: {entry.FullName}");
                        using (var s = entry.Open())
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
