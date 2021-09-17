using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace YWB.AntidetectAccountParser.Helpers
{
    public class CookieHelper
    {
        public static string GetFacebookCookies(string cookies)
        {
            var cookieArray = JArray.Parse(cookies);
            var fbCookies = cookieArray
                .Where(c => c["domain"]?.ToString() == ".facebook.com")
                .Select(c => c.ToString()).ToList();
            if (fbCookies.Count == 0) return string.Empty;
            var lstStr = '[' + string.Join(',', fbCookies) + ']';
            lstStr = Regex.Replace(lstStr.Replace("\r\n", ""), "[ ]+", "");
            return lstStr;
        }

        public static bool HasCUserCookie(string cookies)
        {
            if (string.IsNullOrEmpty(cookies)) return false;
            var cookieArray = JArray.Parse(cookies);
            return cookieArray
                .Any(c => c["name"].ToString().ToLowerInvariant() == "c_user");
        }

        public static string NetscapeCookiesToJSON(string text)
        {
            var cookies = new JArray();
            var lines = text.Split('\n').ToList();

            // iterate over lines
            foreach (var line in lines)
            {
                var tokens = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                dynamic cookie = new JObject();
                switch (tokens.Length)
                {
                    case 7:
                    {
                        cookie.domain = tokens[0].Replace("#HttpOnly_", "").Replace("data-", "");
                        cookie.hostOnly = tokens[1] == "TRUE";
                        cookie.path = tokens[2];
                        cookie.secure = tokens[3] == "TRUE";
                        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() + 3 * 240 * 60 * 60;
                        cookie.expirationDate = timestamp;
                        cookie.name = tokens[5];
                        cookie.value = tokens[6].Trim();
                        break;
                    }
                    case 4:
                    {
                        cookie.domain = tokens[0].Replace("#HttpOnly_", "").Replace("data-", "");
                        cookie.hostOnly = true;
                        cookie.path = tokens[1];
                        cookie.secure = true;
                        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() + 3 * 240 * 60 * 60;
                        cookie.expirationDate = timestamp;
                        cookie.name = tokens[2];
                        cookie.value = tokens[3].Trim();
                        break;
                    }
                    default:
                        continue;

                        // Record the cookie.
                }
                cookies.Add(cookie);
            }
            return cookies.ToString();
        }



        public static bool AreCookiesInBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return false;
            if (base64String.Trim().StartsWith("[") && base64String.Trim().EndsWith("]")) return false;
            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (Exception)
            {
                // Handle the exception
            }
            return false;
        }
    }
}
