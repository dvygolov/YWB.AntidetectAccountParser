using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Browsers
{
    public class DolphinAntyApiService : AbstractAntidetectApiService
    {
        protected override string FileName { get; set; } = "dolphinanty.txt";
        private string _token;
        private string[] _oses = new[] { "windows", "linux", "macos" };

        public async Task<string> CreateNewProfileAsync(string pName, string os, Proxy proxy)
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
            dynamic pr = new JObject();
            pr.type = (proxy.Type == "socks" ? "socks5" : proxy.Type);
            pr.host = proxy.Address;
            pr.port = proxy.Port;
            pr.login = proxy.Login;
            pr.password = proxy.Password;
            if (!string.IsNullOrEmpty(proxy.UpdateLink))
                pr.changeIpUrl = proxy.UpdateLink;
            p.proxy = pr;
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


        public async Task<JObject> GetNewFingerprintAsync(string os)
        {
            var request = new RestRequest("fingerprints/fingerprint", Method.GET);
            request.AddQueryParameter("platform", os);
            request.AddQueryParameter("browser_type", "anty");
            request.AddQueryParameter("browser_version", "97");
            request.AddQueryParameter("type", "fingerprint");
            var res = await ExecuteRequestAsync<JObject>(request);
            return res;
        }

        public async Task<string> GetNewUseragentAsync(string os)
        {
            var request = new RestRequest("fingerprints/useragent", Method.GET);
            request.AddQueryParameter("platform", os);
            request.AddQueryParameter("browser_type", "anty");
            request.AddQueryParameter("browser_version", "97");
            var res = await ExecuteRequestAsync<JObject>(request);
            return res["data"].ToString();
        }

        protected override async Task<List<(string pName, string pId)>> CreateOrChooseProfilesAsync(IList<SocialAccount> accounts)
        {
            var profiles = new List<(string, string)>();
            Console.WriteLine("Choose operating system:");
            var os = SelectHelper.Select(_oses);

            var res = new List<(string, string)>();
            for (int i = 0; i < accounts.Count; i++)
            {
                Console.WriteLine($"Creating profile {accounts[i].Name}...");
                var pId = await CreateNewProfileAsync(accounts[i].Name, os, accounts[i].Proxy);
                Console.WriteLine($"Profile with ID={pId} created!");
                res.Add((accounts[i].Name, pId));
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

        private async Task<T> ExecuteRequestAsync<T>(RestRequest r, string url = "https://anty-api.com")
        {
            var rc = new RestClient(url);
            if (string.IsNullOrEmpty(_token))
                _token = await GetDolphinApiTokenAsync();
            r.AddHeader("Authorization", $"Bearer {_token}");
            T res = default(T);
            int tryCount = 0;
            while (res == null && tryCount < 3)
            {
                var resp = await rc.ExecuteAsync(r, new CancellationToken());
                try
                {
                    res = JsonConvert.DeserializeObject<T>(resp.Content);
                }
                catch
                {
                    Console.WriteLine($"Error deserializing {resp.Content} to {typeof(T)}. Retrying...");
                    await Task.Delay(2000);
                    if (tryCount==2) throw;
                }
                finally
                {
                    tryCount++;
                }
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
                var split = File.ReadAllText(fullPath).Split(':');
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
