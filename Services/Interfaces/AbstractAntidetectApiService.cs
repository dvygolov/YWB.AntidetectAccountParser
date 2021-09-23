using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Services.Interfaces;

namespace YWB.AntidetectAccountParser.Services.Interfaces
{
    public abstract class AbstractAntidetectApiService
    {
        protected readonly IAccountsParser _parser;
        protected readonly IProxyProvider _proxyProvider;

        public AbstractAntidetectApiService(IAccountsParser parser, IProxyProvider proxyProvider)
        {
            _parser = parser;
            _proxyProvider = proxyProvider;
        }

        protected abstract Task<List<(string pName, string pId)>> GetProfilesAsync(List<FacebookAccount> accounts);

        protected abstract Task ImportCookiesAsync(string profileId, string cookies);

        protected abstract Task<bool> SaveItemToNoteAsync(string profileId, FacebookAccount fa);

        public async Task ImportAccountsAsync()
        {
            var accounts = _parser.Parse();
            if (accounts.Count == 0)
            {
                Console.WriteLine("Couldn't find any accounts to import! Unknown format or empty accounts.txt file!");
                return;
            }
            else
                Console.WriteLine($"Found {accounts.Count} accounts.");

            var selectedProfiles = await GetProfilesAsync(accounts);

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
            }
        }
    }
}