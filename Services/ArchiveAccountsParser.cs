using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YWB.AntidetectAccountParser.Helpers;

namespace YWB.AntidetectAccountParser.Services
{
    public class ArchiveAccountsParser : IAccountsParser
    {
        private const string Folder = "logs";
        public List<FacebookAccount> Parse()
        {
            var fullDirPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Folder);
            var res = new List<FacebookAccount>();
            bool isRar = false;
            var files = Directory.GetFiles(fullDirPath, "*.zip");
            if (files.Length == 0)
            {
                files = Directory.GetFiles(fullDirPath, "*.rar");
                isRar = true;
            }

            foreach (var f in files)
            {
                var fa = new FacebookAccount(Path.GetFileNameWithoutExtension(f));
                Console.WriteLine($"Parsing file: {f}");
                if (isRar)
                    RarHelper.Parse(fa, f);
                else
                    ZipHelper.Parse(fa, f);
                if (fa.AllCookies.Any(c => CookieHelper.HasCUserCookie(c)))
                    res.Add(fa);
                else if (fa.Login != null && fa.Password != null)
                {
                    fa.Name = $"PasswordOnly_{fa.Name}";
                    res.Add(fa);
                }
                else
                {
                    var invalid = Path.Combine(fullDirPath, "Invalid");
                    if (!Directory.Exists(invalid)) Directory.CreateDirectory(invalid);
                    File.Move(f, Path.Combine(invalid, Path.GetFileName(f)));
                }
            }

            var finalRes = new List<FacebookAccount>();
            //If we have cookies from multiple accounts we should create an account for each cookie set
            foreach (var fa in res)
            {
                if (fa.AllCookies.Count == 1)
                {
                    finalRes.Add(fa);
                    continue;
                }
                for (int i = 0; i < fa.AllCookies.Count; i++)
                {
                    var cookies = fa.AllCookies[i];
                    var newFa = new FacebookAccount()
                    {
                        Birthday = fa.Birthday,
                        BmLinks = fa.BmLinks,
                        Cookies = cookies,
                        EmailLogin = fa.EmailLogin,
                        EmailPassword = fa.EmailPassword,
                        Logins = fa.Logins,
                        Passwords = fa.Passwords,
                        Token = fa.Token,
                        TwoFactor = fa.TwoFactor,
                        UserAgent = fa.UserAgent,
                        Name = $"{fa.Name}_{i + 1}"
                    };
                    finalRes.Add(newFa);
                }
            }
            return finalRes;
        }
    }
}
