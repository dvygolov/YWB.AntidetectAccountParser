using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private async Task<string> CreateProxyAsync(Proxy p)
        {
            var r = new RestRequest("proxy", Method.POST);
            if (p.Type == "socks") p.Type = "socks5";
            r.AddParameter("type", p.Type);
            r.AddParameter("host", p.Address);
            r.AddParameter("port", p.Port);
            r.AddParameter("login", p.Login);
            r.AddParameter("password", p.Password);
            r.AddParameter("name", DateTime.Now.ToString("G"));
            var res = await ExecuteRequestAsync<JObject>(r);
            return res["data"]["id"].ToString();
        }

        public async Task<string> CreateNewProfileAsync(string pName, string os, string proxyId)
        {
            var fingerprint = await GetNewFingerprintAsync(os);
            var ua = await GetNewUseragentAsync(os);
            var memory = int.Parse(fingerprint["deviceMemory"].ToString());
            if (memory == 0) memory++;
            var cpu = int.Parse(fingerprint["hardwareConcurrency"].ToString());
            if (cpu == 0) cpu++;
            var request = new RestRequest("browser_profiles", Method.POST);
            request.AddParameter("name", pName);
            request.AddParameter("screen[mode]", "manual");
            request.AddParameter("screen[resolution]", $"{fingerprint["screen"]["width"]}x{fingerprint["screen"]["height"]}");
            request.AddParameter("platform", os);
            request.AddParameter("proxy[id]", proxyId);
            request.AddParameter("useragent[mode]", "manual");
            request.AddParameter("useragent[value]", ua);
            request.AddParameter("webrtc[mode]", "altered");
            request.AddParameter("canvas[mode]", "real");
            request.AddParameter("webgl[mode]", "noise");
            request.AddParameter("webglInfo[mode]", "manual");
            request.AddParameter("webglInfo[vendor]", fingerprint["webgl"]["unmaskedVendor"].ToString());
            request.AddParameter("webglInfo[renderer]", fingerprint["webgl"]["unmaskedRenderer"].ToString());
            request.AddParameter("geolocation[mode]", "auto");
            request.AddParameter("timezone[mode]", "auto");
            request.AddParameter("locale[mode]", "auto");
            request.AddParameter("cpu[mode]", "manual");
            request.AddParameter("cpu[value]", cpu);
            request.AddParameter("memory[mode]", "manual");
            request.AddParameter("memory[value]", memory);
            request.AddParameter("doNotTrack", int.Parse(fingerprint["donottrack"].ToString()));
            request.AddParameter("fonts", fingerprint["fonts"].ToString());
            request.AddParameter("mediaDevices[mode]", "manual");
            request.AddParameter("mediaDevices[audioInputs]", new Random().Next(1, 4));
            request.AddParameter("mediaDevices[audioOutputs]", new Random().Next(1, 4));
            request.AddParameter("mediaDevices[videoInputs]", new Random().Next(1, 4));
            request.AddParameter("browserType", "anty");
            var res = await ExecuteRequestAsync<JObject>(request);
            return res["browserProfileId"].ToString();
        }


        public async Task<JObject> GetNewFingerprintAsync(string os)
        {
            var request = new RestRequest("fingerprints/fingerprint", Method.GET);
            request.AddQueryParameter("platform", os);
            request.AddQueryParameter("browser_type", "anty");
            request.AddQueryParameter("browser_version", "undefined");
            var res = await ExecuteRequestAsync<JObject>(request);
            return res;
        }

        public async Task<string> GetNewUseragentAsync(string os)
        {
            var request = new RestRequest("fingerprints/useragent", Method.GET);
            request.AddQueryParameter("platform", os);
            var res = await ExecuteRequestAsync<JObject>(request);
            return res["data"].ToString();
        }

        protected override async Task<List<(string pName, string pId)>> GetProfilesAsync(List<FacebookAccount> accounts)
        {
            var profiles = new List<(string, string)>();
            var proxies = _proxyProvider.Get();
            var namePrefix = string.Empty;
            if (!accounts.All(a => !string.IsNullOrEmpty(a.Name)))
            {
                Console.Write("Enter profile name prefix:");
                namePrefix = Console.ReadLine();
            }
            Console.WriteLine("Choose operating system:");
            var os = SelectHelper.Select(new[] { "windows", "linux", "macos" });

            var proxyIds = new Dictionary<Proxy, string>();
            var res = new List<(string, string)>();
            for (int i = 0; i < accounts.Count; i++)
            {
                var proxyIndex = i < proxies.Count - 1 ? i : i % proxies.Count;
                var p = proxies[proxyIndex];
                if (!proxyIds.ContainsKey(p))
                {
                    Console.WriteLine("Adding proxy...");
                    var proxyId = await CreateProxyAsync(p);
                    proxyIds.Add(p, proxyId);
                    Console.WriteLine("Proxy added!");
                }
                var pName = string.IsNullOrEmpty(accounts[i].Name) ? $"{namePrefix}{i}" : accounts[i].Name;
                Console.WriteLine($"Creating profile {pName}...");
                var pId = await CreateNewProfileAsync(pName, os, proxyIds[p]);
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
            var request = new RestRequest($"browser_profiles/{profileId}", Method.PATCH);
            request.AddParameter("notes[content]", fa.ToString(true));
            var res = await ExecuteRequestAsync<JObject>(request);
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
