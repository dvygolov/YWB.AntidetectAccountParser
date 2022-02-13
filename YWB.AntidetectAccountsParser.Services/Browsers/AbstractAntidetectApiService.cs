using Microsoft.Extensions.Logging;
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
        protected readonly ILogger<AbstractAntidetectApiService> _logger;

        public AbstractAntidetectApiService(string credentials, ILoggerFactory lf)
        {
            _credentials = credentials;
            _logger = lf.CreateLogger<AbstractAntidetectApiService>();
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
                _logger.LogInformation("Couldn't find any accounts to import! Unknown format or empty accounts file!");
                return null;
            }
            else
                _logger.LogInformation($"Found {count} accounts.");

            AccountNamesHelper.Process(accounts, fs);

            Dictionary<string, SocialAccount> profiles = new Dictionary<string, SocialAccount>();

            foreach (SocialAccount account in accounts)
            {
                _logger.LogInformation($"Creating profile {account.Name}...");
                var pId = await CreateNewProfileAsync(account, fs.Os, fs.Group);
                _logger.LogInformation($"Profile with ID={pId} created!");
                if (!string.IsNullOrEmpty(account.Cookies))
                {
                    _logger.LogInformation($"Importing {account.Login} account's cookies to {account.Name} profile...");

                    if (CookieHelper.AreCookiesInBase64(account.Cookies))
                    {
                        account.Cookies = Encoding.UTF8.GetString(Convert.FromBase64String(account.Cookies));
                    }
                    await ImportCookiesAsync(pId, account.Cookies);
                }

                await SaveItemToNoteAsync(pId, account);
                _logger.LogInformation($"Profile {account.Name} saved!");
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