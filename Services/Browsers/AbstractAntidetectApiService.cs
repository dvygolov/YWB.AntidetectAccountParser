using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Browsers
{
    public abstract class AbstractAntidetectApiService
    {
        protected abstract Task<List<(string pName, string pId)>> CreateOrChooseProfilesAsync(
            IList<SocialAccount> accounts);

        protected abstract Task ImportCookiesAsync(string profileId, string cookies);

        protected abstract Task<bool> SaveItemToNoteAsync(string profileId, SocialAccount sa);

        public async Task<Dictionary<string, SocialAccount>> ImportAccountsAsync(IList<SocialAccount> accounts)
        {
            var res = new Dictionary<string, SocialAccount>();
            if (accounts.Count == 0)
            {
                Console.WriteLine("Couldn't find any accounts to import! Unknown format or empty accounts.txt file!");
                return null;
            }
            else
                Console.WriteLine($"Found {accounts.Count} accounts.");

            var selectedProfiles = await CreateOrChooseProfilesAsync(accounts);

            for (int i = 0; i < accounts.Count; i++)
            {
                string pId = selectedProfiles[i].pId;
                string pName = selectedProfiles[i].pName;

                if (!string.IsNullOrEmpty(accounts[i].Cookies))
                {
                    Console.WriteLine($"Importing {accounts[i].Login} account's cookies to {pName} profile...");

                    if (CookieHelper.AreCookiesInBase64(accounts[i].Cookies))
                    {
                        accounts[i].Cookies = Encoding.UTF8.GetString(Convert.FromBase64String(accounts[i].Cookies));
                    }
                    await ImportCookiesAsync(pId, accounts[i].Cookies);
                }

                await SaveItemToNoteAsync(pId, accounts[i]);
                Console.WriteLine("Note saved!");
                res.Add(selectedProfiles[i].pId, accounts[i]);
            }
            return res;
        }
    }
}