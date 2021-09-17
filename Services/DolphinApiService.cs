using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Interfaces;

namespace YWB.AntidetectAccountParser.Services
{
    public class DolphinApiService : AbstractAntidetectApiService
    {
        private string _token;
        public DolphinApiService(IAccountsParser parser, IProxyProvider proxyProvider) : base(parser, proxyProvider) { }

        public async Task<string> CreateNewProfileAsync(string pName, string os, Proxy p)
        {
            var request = new RestRequest("browser_profiles",Method.POST);
            request.AddParameter("name", pName);
            request.AddParameter("platform", os);
            request.AddParameter("proxy[type]", p.Type);
            request.AddParameter("proxy[host]", p.Address);
            request.AddParameter("proxy[port]", p.Port);
            request.AddParameter("proxy[login]", p.Login);
            request.AddParameter("proxy[password]", p.Password);
            var res=await ExecuteRequestAsync<JObject>(request);
            return res["browserProfileId"].ToString();
        }

        protected override async Task<List<(string pName, string pId)>> GetProfilesAsync(List<FacebookAccount> accounts)
        {
            var profiles = new List<(string, string)>();
            var proxy = _proxyProvider.Get();
            var namePrefix = string.Empty;
            if (!accounts.All(a => !string.IsNullOrEmpty(a.Name)))
            {
                Console.Write("Enter profile name prefix:");
                namePrefix = Console.ReadLine();
            }
            Console.WriteLine("Choose operating system:");
            var os = SelectHelper.Select(new[] { "windows", "linux", "macos" });
            var res = new List<(string, string)>();
            for (int i = 0; i < accounts.Count; i++)
            {
                var pName = string.IsNullOrEmpty(accounts[i].Name) ? $"{namePrefix}{i}" : accounts[i].Name;
                Console.WriteLine($"Creating profile {pName}...");
                var pId = await CreateNewProfileAsync(pName, os, proxy);
                Console.WriteLine($"Profile with ID={pId} created!");
                res.Add((pName, pId));
            }
            return res;
        }

        protected override async Task ImportCookiesAsync(string profileId, string cookies)
        {
            var request = new RestRequest($"sync/{profileId}/cookies", Method.POST);
            var body = @$"{{""cookies"":{cookies}}}";
            request.AddParameter("text/plain", body, ParameterType.RequestBody);
            request.AddHeader("Content-Type", "application/json");
            var res = await ExecuteRequestAsync<JObject>(request);
        }

        protected override async Task<bool> SaveItemToNoteAsync(string profileId, FacebookAccount fa)
        {
            var request = new RestRequest($"browser_profiles/{profileId}",Method.PATCH);
            request.AddParameter("notes[content]", fa.ToString(true));
            var res=await ExecuteRequestAsync<JObject>(request);
            return true;
        }

        private async Task<T> ExecuteRequestAsync<T>(RestRequest r, string url = "https://anty-api.com")
        {
            var rc = new RestClient(url);
            if (string.IsNullOrEmpty(_token))
                _token = await GetDolphinApiTokenAsync();
            r.AddHeader("Authorization", $"Bearer {_token}");
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

        private async Task<string> GetDolphinApiTokenAsync()
        {
            Console.Write("Enter your Dolphin Anty login:");
            var login = Console.ReadLine();
            Console.Write("Enter your Dolphin Anty password:");
            var password = Console.ReadLine();
            var client = new RestClient("https://anty-api.com/auth/login");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddParameter("username", login);
            request.AddParameter("password", password);
            var response = await client.ExecuteAsync(request, new CancellationToken());
            var res = JObject.Parse(response.Content);
            return res["token"].ToString();
        }
    }
}
