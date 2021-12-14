using System;
using System.Text;
using YWB.AntidetectAccountParser.Helpers;

namespace YWB.AntidetectAccountParser.Model.Accounts.Actions
{
    public class CookiesAccountAction<T> : AccountAction<T> where T : SocialAccount
    {
        public CookiesAccountAction()
        {
            Condition = (fileName) => fileName.Contains("cookie");
            Action = (stream,fa) => ExtractCookies(stream,fa);
            Message = "Found file with cookies: ";
        }

        private void ExtractCookies(System.IO.Stream s,T sa)
        {
            var text = Encoding.UTF8.GetString(s.ReadAllBytes());
            var allCookies = !text.Trim().StartsWith('[') ? CookieHelper.NetscapeCookiesToJSON(text) : text;
            string cookies = CookieHelper.GetDomainCookies(allCookies, sa.Domain);
            if (!string.IsNullOrEmpty(cookies))
                if (sa.AddCookies(cookies))
                    Console.WriteLine("Found cookies!");
        }
    }
}
