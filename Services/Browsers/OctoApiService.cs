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
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Browsers
{
    public class OctoApiService : AbstractAntidetectApiService
    {
        private const string FileName = "octo.txt";
        private const string ApiUrl = "https://app.octobrowser.net/api/v2/automation/";
        private string _token;
        private string[] _oses = new[] { "windows", "linux", "macos" };

        private async Task<List<Proxy>> GetExistingProxiesAsync()
        {
            var r = new RestRequest("proxies", Method.GET);
            JObject res = await ExecuteRequestAsync<JObject>(r);
            return res["data"].Select((dynamic p) => new Proxy()
            {
                Id = p.uuid,
                Type = p.type,
                Address = p.host,
                Port = p.port,
                Login = p.login,
                Password = p.password
            }).ToList();
        }

        private async Task<string> CreateOrGetProxyAsync(Proxy p)
        {
            var allProxies = await GetExistingProxiesAsync();
            var allProxiesDict = new Dictionary<Proxy, string>();
            allProxies.ForEach(pr =>
            {
                if (!allProxiesDict.ContainsKey(pr))
                    allProxiesDict.Add(pr, pr.Id);
            });

            if (allProxiesDict.ContainsKey(p))
            {
                Console.WriteLine("Found existing proxy!");
                return allProxiesDict[p];
            }
            var r = new RestRequest("proxies", Method.POST);
            if (p.Type == "socks5" || p.Type == "socks4") p.Type = "socks";
            dynamic pjson = new JObject();
            pjson.type = p.Type;
            pjson.host = p.Address;
            pjson.port = int.Parse(p.Port);
            pjson.login = p.Login;
            pjson.password = p.Password;
            pjson.title = DateTime.Now.ToString("G");
            r.AddParameter("application/json", pjson, ParameterType.RequestBody);

            var res = await ExecuteRequestAsync<JObject>(r);
            if (!res["success"]?.Value<bool>() ?? false)
                throw new Exception(res["error"].ToString());
            Console.WriteLine("Proxy added!");
            return res["data"]["uuid"].ToString();
        }

        private async Task<List<AccountGroup>> GetExistingTagsAsync()
        {
            var r = new RestRequest("tags", Method.GET);
            var json = await ExecuteRequestAsync<JObject>(r);
            return json["data"].Select((dynamic g)=> new AccountGroup()
            {
                Id = g.uuid,
                Name = g.name
            }).ToList();
        }

        protected override Task<AccountGroup> AddNewGroupAsync()
        {
            Console.Write("Enter tag name:");
            var tagName = Console.ReadLine();
            return Task.FromResult(new AccountGroup() { Id = "new", Name = tagName });
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
            p.mediaDevices.audioInputs = StaticRandom.Instance.Next(1, 4);
            p.mediaDevices.audioOutputs = StaticRandom.Instance.Next(1, 4);
            p.mediaDevices.videoInputs = StaticRandom.Instance.Next(1, 4);
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


        protected override async Task<List<(string pName, string pId)>> CreateOrChooseProfilesAsync(IList<SocialAccount> accounts)
        {
            var profiles = new List<(string, string)>();
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
                if (!proxyIds.ContainsKey(accounts[i].Proxy))
                {
                    Console.WriteLine("Adding proxy...");
                    var proxyId = await CreateOrGetProxyAsync(accounts[i].Proxy);
                    proxyIds.Add(accounts[i].Proxy, proxyId);
                }
                var pName = string.IsNullOrEmpty(accounts[i].Name) ? $"{namePrefix}{i}" : accounts[i].Name;
                accounts[i].Name = pName;
                Console.WriteLine($"Creating profile {pName}...");
                var pId = await CreateNewProfileAsync(pName, os, proxyIds[accounts[i].Proxy]);
                Console.WriteLine($"Profile with ID={pId} created!");
                res.Add((pName, pId));
            }
            return res;
        }


        protected override async Task ImportCookiesAsync(string profileId, string cookies)
        {
            var request = new RestRequest($"{profileId}/import_cookies", Method.POST);
            var body = @$"{{""cookies"":{cookies}}}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            var res = await ExecuteRequestAsync<JObject>(request);
            if (!res["success"]?.Value<bool>() ?? false)
                throw new Exception(res["error"].ToString());
        }

        protected override async Task<bool> SaveItemToNoteAsync(string profileId, SocialAccount fa)
        {
            var request = new RestRequest($"browser_profiles/{profileId}", Method.PATCH);
            request.AddParameter("notes[content]", fa.ToString(true));
            request.AddParameter("notes[color]", "blue");
            request.AddParameter("notes[style]", "text");
            request.AddParameter("notes[icon]", null);
            var res = await ExecuteRequestAsync<JObject>(request);
            return true;
        }

        private async Task<T> ExecuteRequestAsync<T>(RestRequest r)
        {
            var rc = new RestClient(ApiUrl);
            if (string.IsNullOrEmpty(_token)) _token = GetOctoApiToken();
            r.AddHeader("Content-Type", "application/json");
            r.AddHeader("X-Octo-Api-Token", _token);
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

        private string GetOctoApiToken()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(dir, FileName);
            if (File.Exists(fullPath))
            {
                return File.ReadAllText(fullPath);
            }
            else
            {
                Console.Write("Enter your Octo browsers' API Token:");
                var token = Console.ReadLine();
                return token;
            }
        }
    }
}
