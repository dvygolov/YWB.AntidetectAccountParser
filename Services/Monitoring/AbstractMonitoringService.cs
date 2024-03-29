﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Monitoring
{
    public abstract class AbstractMonitoringService
    {
        protected string _token;
        protected string _apiUrl;

        protected abstract Task SetTokenAndApiUrlAsync();
        protected abstract void AddAuthorization(RestRequest r);
        protected abstract Task<List<AccountGroup>> GetExistingGroupsAsync();
        protected abstract Task<AccountGroup> AddNewGroupAsync();
        protected abstract Task<List<Proxy>> GetExistingProxiesAsync();
        protected abstract Task<string> AddProxyAsync(Proxy p);
        protected abstract Task<bool> AddAccountAsync(FacebookAccount acc, AccountGroup g, string proxyId);
        public async Task AddAccountsAsync(List<FacebookAccount> accounts)
        {
            AccountNamesHelper.Process(accounts);
            var groups = await GetExistingGroupsAsync();
            Console.WriteLine("Do you want to add your accounts to group/tag?");
            var group = await SelectHelper.SelectWithCreateAsync(groups, g => g.Name, AddNewGroupAsync, true);
            Console.WriteLine("Getting existing proxies...");

            var existingProxies = await GetExistingProxiesAsync();
            var existingProxiesDict = new Dictionary<Proxy, string>();
            existingProxies.ForEach(pr =>
            {
                if (!existingProxiesDict.ContainsKey(pr))
                    existingProxiesDict.Add(pr, pr.Id);
            });

            //We should add accounts with the same token only once
            var distinct = accounts.GroupBy(a => a.Token).Select(g => g.First()).ToList();
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
                var success = await AddAccountAsync(acc, group, proxyId);
                if (success)
                    Console.WriteLine($"Account {acc.Name} added!");
            }
        }

        protected async Task<T> ExecuteRequestAsync<T>(RestRequest r)
        {
            if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_apiUrl))
                await SetTokenAndApiUrlAsync();
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
