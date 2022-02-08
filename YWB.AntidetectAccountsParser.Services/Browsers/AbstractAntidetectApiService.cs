using System.Text;
using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.Helpers;

namespace YWB.AntidetectAccountsParser.Services.Browsers
{
    public abstract class AbstractAntidetectApiService : IAccountsImporter
    {
        protected readonly string _credentials;

        public AbstractAntidetectApiService(string credentials)
        {
            _credentials = credentials;
        }

        public abstract Task<string> CreateNewProfileAsync(SocialAccount acc, string os, AccountGroup group);
        protected abstract Task ImportCookiesAsync(string profileId, string cookies);
        protected abstract Task<bool> SaveItemToNoteAsync(string profileId, SocialAccount sa);
        public abstract Task<List<AccountGroup>> GetExistingGroupsAsync();
        public abstract Task<AccountGroup> AddNewGroupAsync(string groupName);
        public abstract List<string> GetOSes();

        public async Task<Dictionary<string, SocialAccount>> ImportAccountsAsync(
            IEnumerable<SocialAccount> accounts, FlowSettings fs)
        {
            var res = new Dictionary<string, SocialAccount>();
            var count = accounts.Count();
            if (count == 0)
            {
                Console.WriteLine("Couldn't find any accounts to import! Unknown format or empty accounts file!");
                return null;
            }
            else
                Console.WriteLine($"Found {count} accounts.");

            AccountNamesHelper.Process(accounts, fs);

            Dictionary<string, SocialAccount> profiles = new Dictionary<string, SocialAccount>();

            foreach (SocialAccount account in accounts)
            {
                Console.WriteLine($"Creating profile {account.Name}...");
                var pId = await CreateNewProfileAsync(account, fs.Os, fs.Group);
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
                profiles.Add(pId, account);
            }
            return res;
        }

        public List<string> GetOsList()
        {
            return GetOSes();
        }
    }
}