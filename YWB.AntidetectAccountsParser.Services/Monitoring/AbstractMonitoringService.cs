using Newtonsoft.Json;
using RestSharp;
using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.Helpers;

namespace YWB.AntidetectAccountsParser.Services.Monitoring
{
    public abstract class AbstractMonitoringService : IAccountsImporter
    {
        protected string _apiUrl;
        protected string _token;
        protected readonly string _credentials;

        public AbstractMonitoringService(string credentials)
        {
            _credentials = credentials;
        }
        protected abstract void SetTokenAndApiUrl();
        protected abstract void AddAuthorization(RestRequest r);
        public abstract Task<List<AccountGroup>> GetExistingGroupsAsync();
        public abstract Task<AccountGroup> AddNewGroupAsync(string groupName);
        protected abstract Task<List<Proxy>> GetExistingProxiesAsync();
        protected abstract Task<string> AddProxyAsync(Proxy p);
        protected abstract Task<bool> AddAccountAsync(FacebookAccount acc, AccountGroup g, string proxyId);
        public async Task<Dictionary<string, SocialAccount>> ImportAccountsAsync(IEnumerable<SocialAccount> accounts, FlowSettings fs)
        {
            AccountNamesHelper.Process(accounts, fs);
            Console.WriteLine("Getting existing proxies...");
            var existingProxies = await GetExistingProxiesAsync();
            var existingProxiesDict = new Dictionary<Proxy, string>();
            existingProxies.ForEach(pr =>
            {
                if (!existingProxiesDict.ContainsKey(pr))
                    existingProxiesDict.Add(pr, pr.Id);
            });

            //We should add accounts with the same token only once
            var distinct = accounts.GroupBy(a => (a as FacebookAccount).Token).Select(g => g.First()).ToList();
            foreach (var acc in distinct)
            {
                string proxyId;
                if (existingProxiesDict.ContainsKey(acc.Proxy))
                {
                    proxyId = existingProxiesDict[acc.Proxy];
                    Console.WriteLine($"Found existing proxy for {acc.Proxy}!");
                }
                else
                {
                    Console.WriteLine($"Adding proxy {acc.Proxy}...");
                    proxyId = await AddProxyAsync(acc.Proxy);
                    existingProxiesDict.Add(acc.Proxy, proxyId);
                    Console.WriteLine($"Proxy {acc.Proxy} added!");
                }
                Console.WriteLine($"Adding account {acc.Name}...");
                var success = await AddAccountAsync(acc as FacebookAccount, fs.Group, proxyId);
                if (success)
                    Console.WriteLine($"Account {acc.Name} added!");
            }
            return null;
        }

        protected async Task<T> ExecuteRequestAsync<T>(RestRequest r)
        {
            if (string.IsNullOrEmpty(_credentials) || string.IsNullOrEmpty(_apiUrl))
                SetTokenAndApiUrl();
            var rc = new RestClient(_apiUrl);
            AddAuthorization(r);
            var resp = await rc.ExecuteAsync(r, new CancellationToken());
            T res;
            try
            {
                res = JsonConvert.DeserializeObject<T>(resp.Content);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error deserializing {resp.Content} to {typeof(T)}: {e}");
                throw;
            }
            return res;
        }

    }
}
