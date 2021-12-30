using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Browsers
{
    public abstract class AbstractAntidetectApiService:IAccountsImporter
    {
        protected abstract string FileName { get; set; }
        protected abstract Task<List<(string pName, string pId)>> CreateProfilesAsync(IEnumerable<SocialAccount> accounts);

        protected abstract Task ImportCookiesAsync(string profileId, string cookies);

        protected abstract Task<bool> SaveItemToNoteAsync(string profileId, SocialAccount sa);

        public async Task<Dictionary<string, SocialAccount>> ImportAccountsAsync(IEnumerable<SocialAccount> accounts)
        {
            var res = new Dictionary<string, SocialAccount>();
            var count = accounts.Count();
            if (count == 0)
            {
                Console.WriteLine("Couldn't find any accounts to import! Unknown format or empty accounts.txt file!");
                return null;
            }
            else
                Console.WriteLine($"Found {count} accounts.");


            AccountNamesHelper.Process(accounts);

            var selectedProfiles = await CreateProfilesAsync(accounts);

            int i = 0;
            foreach(var account in accounts)
            {
                string pId = selectedProfiles[i].pId;
                string pName = selectedProfiles[i].pName;

                if (!string.IsNullOrEmpty(account.Cookies))
                {
                    Console.WriteLine($"Importing {account.Login} account's cookies to {pName} profile...");

                    if (CookieHelper.AreCookiesInBase64(account.Cookies))
                    {
                        account.Cookies = Encoding.UTF8.GetString(Convert.FromBase64String(account.Cookies));
                    }
                    await ImportCookiesAsync(pId, account.Cookies);
                }

                await SaveItemToNoteAsync(pId, account);
                Console.WriteLine("Note saved!");
                res.Add(selectedProfiles[i].pId, account);
                i++;
            }
            return res;
        }
    }
}