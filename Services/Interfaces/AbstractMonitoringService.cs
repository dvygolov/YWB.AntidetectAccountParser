using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Model;

namespace YWB.AntidetectAccountParser.Services.Interfaces
{
    public abstract class AbstractMonitoringService
    {
        protected string _token;
        protected string _apiUrl;

        protected abstract Task SetTokenAndApiUrlAsync();
        protected abstract void AddAuthorization(RestRequest r);
        protected abstract Task<List<Proxy>> GetExistringProxiesAsync();
        protected abstract Task<string> AddProxyAsync(Proxy p);
        protected abstract Task AddAccountAsync(FacebookAccount acc, string proxyId);
        public async Task AddAccountsAsync(List<FacebookAccount> accounts)
        {
            var existingProxies = (await GetExistringProxiesAsync()).ToDictionary(p=>p,p=>p.Id);
            foreach (var acc in accounts)
            {
                var proxyId = existingProxies.ContainsKey(acc.Proxy) ? 
                    existingProxies[acc.Proxy] : await AddProxyAsync(acc.Proxy);
                await AddAccountAsync(acc, proxyId);
            }
        }

        protected async Task<T> ExecuteRequestAsync<T>(RestRequest r)
        {
            if (string.IsNullOrEmpty(_token)||string.IsNullOrEmpty(_apiUrl))
                await SetTokenAndApiUrlAsync();
            var rc = new RestClient(_apiUrl);
            AddAuthorization(r);
            var resp = await rc.ExecuteAsync(r, new CancellationToken());
            T res = default(T);
            try
            {
                res = JsonConvert.DeserializeObject<T>(resp.Content);
            }
            catch (Exception)
            {
                Console.WriteLine($"Error deserializing {resp.Content} to {typeof(T)}");
                throw;
            }
            return res;
        }

    }
}
