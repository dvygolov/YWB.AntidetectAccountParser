using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YWB.AntidetectAccountParser.Model.Accounts;
using YWB.AntidetectAccountParser.Model.Accounts.Actions;

namespace YWB.AntidetectAccountParser.Services.Parsers
{
    public class GoogleArchivesAccountsParser : AbstractArchivesAccountsParser<SocialAccount>
    {
        public override ActionsFacade<SocialAccount> GetActions(string filePath)
        {
            var sa = new SocialAccount(Path.GetFileNameWithoutExtension(filePath));
            Console.WriteLine($"Parsing file: {filePath}");
            return new ActionsFacade<SocialAccount>()
            {
                Account = sa,
                AccountActions = new List<AccountAction<SocialAccount>>()
                {
                    new PasswordAccountAction<SocialAccount>(),
                    new CookiesAccountAction<SocialAccount>()
                }
            };
        }

        public override AccountValidity IsValid(SocialAccount fa)
        {
            if (fa.AllCookies.Any())
                return AccountValidity.Valid;
            else if (fa.Login != null && fa.Password != null)
                return AccountValidity.PasswordOnly;
            else
                return AccountValidity.Invalid;
        }

        public override IEnumerable<SocialAccount> MultiplyCookies(IEnumerable<SocialAccount> accounts)
        {
            var finalRes = new List<SocialAccount>();
            //If we have cookies from multiple accounts we should create an account for each cookie set
            foreach (var fa in accounts)
            {
                if (fa.AllCookies.Count == 1)
                {
                    finalRes.Add(fa);
                    continue;
                }
                for (int i = 0; i < fa.AllCookies.Count; i++)
                {
                    var cookies = fa.AllCookies[i];
                    var newFa = new SocialAccount()
                    {
                        Cookies = cookies,
                        Logins = fa.Logins,
                        Passwords = fa.Passwords,
                        Name = $"{fa.Name}_{i + 1}"
                    };
                    finalRes.Add(newFa);
                }
            }
            return finalRes;
        }
    }
}
