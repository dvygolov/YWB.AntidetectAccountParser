using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Interfaces;

namespace YWB.AntidetectAccountParser.Services
{
    public class DolphinApiService : AbstractAntidetectApiService
    {
        private const string FileName = "dolphinanty.txt";
        private string _token;
        private string[] _oses = new[] { "windows", "linux", "macos" };
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
            var fp = await GetNewFingerprintAsync(os);
            var ua = await GetNewUseragentAsync(os);
            var memory = int.Parse(fp["deviceMemory"].ToString());
            if (memory == 0) memory++;
            var cpu = int.Parse(fp["hardwareConcurrency"].ToString());
            if (cpu == 0) cpu++;
            var request = new RestRequest("browser_profiles", Method.POST);
            request.AddHeader("Content-Type", "application/json;charset=UTF-8");
            dynamic p = new JObject();
            p.name = pName;
            p.screen = new JObject();
            p.screen.mode = "manual";
            p.screen.resolution = $"{fp["screen"]["width"]}x{fp["screen"]["height"]}";
            p.platform = os;
            p.platformName = fp["platform"];
            p.osVersion = fp["os"]["version"];
            p.proxy = new JObject();
            p.proxy.id = proxyId;
            p.useragent = new JObject();
            p.useragent.mode = "manual";
            p.useragent.value = ua;
            p.webrtc = new JObject();
            p.webrtc.mode = "altered";
            p.webrtc.ipAddress = "";
            p.canvas = new JObject();
            p.canvas.mode = "real";
            p.webgl = new JObject();
            p.webgl.mode = "noise";
            p.webglInfo = new JObject();
            p.webglInfo.mode = "manual";
            p.webglInfo.vendor = fp["webgl"]["unmaskedVendor"];
            p.webglInfo.renderer = fp["webgl"]["unmaskedRenderer"];
            p.clientRect = new JObject();
            p.clientRect.mode = "real";
            p.geolocation = new JObject();
            p.geolocation.mode = "auto";
            p.timezone = new JObject();
            p.timezone.mode = "auto";
            p.timezone.value = "";
            p.locale = new JObject();
            p.locale.mode = "auto";
            p.locale.value = "";
            p.cpu = new JObject();
            p.cpu.mode = "manual";
            p.cpu.value = cpu.ToString();
            p.memory = new JObject();
            p.memory.mode = "manual";
            p.memory.value = memory.ToString();
            p.doNotTrack = fp["donottrack"];
            p.fonts = fp["fonts"];
            p.mediaDevices = new JObject();
            p.mediaDevices.mode = "manual";
            p.mediaDevices.audioInputs = new Random().Next(1, 4);
            p.mediaDevices.audioOutputs = new Random().Next(1, 4);
            p.mediaDevices.videoInputs = new Random().Next(1, 4);
            p.browserType = "anty";
            p.mainWebsite = "facebook";
            p.appCodeName = fp["appCodeName"];
            p.cpuArchitecture = fp["cpu"]["architecture"];
            p.vendor = fp["vendor"];
            p.vendorSub = fp["vendorSub"];
            p.product = fp["product"];
            p.productSub = fp["productSub"];
            p.connectionDownlink = fp["connection"]["downlink"];
            p.connectionRtt = fp["connection"]["rtt"];
            p.connectionEffectiveType = fp["connection"]["effectiveType"];
            p.connectionSaveData = fp["connection"]["saveData"];
            p.ports = new JObject();
            p.ports.mode = "protect";
            p.ports.blacklist = "3389,5900,5800,7070,6568,5938";
            p.statusId = 0;
            p.tags = new JArray();

            request.AddParameter("application/json", p.ToString(), ParameterType.RequestBody);
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
            var os = SelectHelper.Select(_oses);

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
            request.AddParameter("notes[color]", "blue");
            request.AddParameter("notes[style]", "text");
            request.AddParameter("notes[icon]", null);
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
            (string login, string password) = GetLoginAndPassword();
            var client = new RestClient("https://anty-api.com/auth/login");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddParameter("username", login);
            request.AddParameter("password", password);
            var response = await client.ExecuteAsync(request, new CancellationToken());
            var res = JObject.Parse(response.Content);
            return res["token"].ToString();
        }

        private (string login, string password) GetLoginAndPassword()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(dir, FileName);
            if (File.Exists(fullPath))
            {
                var split=File.ReadAllText(fullPath).Split(':');
                return (split[0], split[1]);
            }
            else
            {
                Console.Write("Enter your Dolphin Anty login:");
                var login = Console.ReadLine();
                Console.Write("Enter your Dolphin Anty password:");
                var password = Console.ReadLine();
                return (login, password);
            }
        }
    }
}
