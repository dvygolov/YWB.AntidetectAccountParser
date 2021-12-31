using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Browsers
{
    public abstract class AbstractAntidetectApiService:IAccountsImporter
    {
        protected abstract string FileName { get; set; }
        public abstract Task<string> CreateNewProfileAsync(string pName, string os, Proxy proxy, AccountGroup group);
        protected abstract Task ImportCookiesAsync(string profileId, string cookies);
        protected abstract Task<bool> SaveItemToNoteAsync(string profileId, SocialAccount sa);
        public abstract Task<List<AccountGroup>> GetExistingGroupsAsync();
        public abstract Task<AccountGroup> AddNewGroupAsync();
        public abstract List<string> GetOSes();

        public async Task<Dictionary<string, SocialAccount>> ImportAccountsAsync(
            IEnumerable<SocialAccount> accounts, FlowSettings fs)
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

            AccountNamesHelper.Process(accounts,fs);

            Dictionary<string, SocialAccount> profiles = new Dictionary<string, SocialAccount>();

            foreach (SocialAccount account in accounts)
            {
                Console.WriteLine($"Creating profile {account.Name}...");
                var pId = await CreateNewProfileAsync(account.Name, fs.Os, account.Proxy, fs.Group);
                Console.WriteLine($"Profile with ID={pId} created!");
                if (!string.IsNullOrEmpty(account.Cookies))
                {
                    Console.WriteLine($"Importing {account.Login} account's cookies to {account.Name} profile...");

                    if (CookieHelper.AreCookiesInBase64(account.Cookies))
                    {
                        account.Cookies = Encoding.UTF8.GetString(Convert.FromBase64String(account.Cookies));
                    }
                    await ImportCookiesAsync(pId, account.Cookies);
                }

                await SaveItemToNoteAsync(pId, account);
                Console.WriteLine($"Profile {account.Name} saved!");
                profiles.Add(pId,account);
            }
            return res;
        }
    }
}