using System;
using System.Linq;
using System.Text;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model;

namespace YWB.AntidetectAccountParser.Services.Interfaces
{
    public abstract class AbstractArchiveParser
    {
        public abstract void Parse(FacebookAccount fa, string filePath);
        protected void ExtractCookies(FacebookAccount fa, System.IO.Stream s)
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

        protected void ExtractLoginAndPassword(FacebookAccount fa, System.IO.Stream s)
        {
            var lines = Encoding.UTF8.GetString(s.ReadAllBytes()).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            int index = -1;
            while ((index = lines.FindIndex(index + 1, l => l.ToLowerInvariant().Contains("facebook"))) != -1)
            {
                if (index + 2 >= lines.Count) continue;
                var login = lines[index + 1].Split(' ')[1];
                var password = lines[index + 2].Split(' ')[1];
                if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                {
                    if (fa.AddLoginPassword(login, password))
                        Console.WriteLine("Found Facebook login/password!");
                    index += 2;
                }
            }
        }

        protected void ExtractToken(FacebookAccount fa, System.IO.Stream s)
        {
            var content = Encoding.UTF8.GetString(s.ReadAllBytes()).Trim();
            if (content.StartsWith("EAAB"))
            {
                fa.Token = content;
                Console.WriteLine("Found Facebook Access Token!");
            }
            if (content.StartsWith("EAAG"))
            {
                fa.BmToken = content;
                Console.WriteLine("Found Business Manager Access Token!");
            }
        }
    }
}
