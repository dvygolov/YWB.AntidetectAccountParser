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
using YWB.AntidetectAccountParser.Model.Accounts;

namespace YWB.AntidetectAccountParser.Services.Browsers
{
    public class AdsPowerApiService : AbstractAntidetectApiService
    {
        private string _token;
        private string _cpl;
        protected override string FileName { get; set; } = "adspower.txt";
        private List<string> _oses = new List<string> { "Windows", "Mac OS X", "Linux" };
        private List<string> _cpu = new List<string> { "2", "4", "6", "8", "16" };
        private List<string> _memory = new List<string> { "2", "4", "6", "8" };

        protected override async Task<List<(string pName, string pId)>> CreateOrChooseProfilesAsync(IList<SocialAccount> accounts)
        {
            var profiles = new List<(string, string)>();
            Console.WriteLine("Choose operating system:");
            var os = SelectHelper.Select(_oses);

            var res = new List<(string, string)>();
            for (int i = 0; i < accounts.Count; i++)
            {
                Console.WriteLine($"Creating profile {accounts[i].Name}...");
                var pId = await CreateNewProfileAsync(os, accounts[i]);
                Console.WriteLine($"Profile with ID={pId} created!");
                res.Add((accounts[i].Name, pId));
            }
            return res;

        }

        private async Task<string> CreateNewProfileAsync(string os, SocialAccount sa)
        {
            var r = new RestRequest("fbcc/user/rand-get-user-agent", Method.POST);
            r.AddParameter("system", os);
            dynamic json = await ExecuteRequestAsync<JObject>(r);
            string ua = json.data.ua;

            r = new RestRequest("fbcc/user/random-webgl-config", Method.POST);
            r.AddParameter("ua", ua);
            json = await ExecuteRequestAsync<JObject>(r);
            string renderer = json.data.unmasked_renderer;
            string vendor = json.data.unmasked_vendor;

            r = new RestRequest("fbcc/user/single-import-user", Method.POST);
            r.AddParameter("batch_id", "0");
            r.AddParameter("name", sa.Name);
            r.AddParameter("domain_name", sa.Domain);
            if (!string.IsNullOrEmpty(sa.Login) && !string.IsNullOrEmpty(sa.Password))
            {
                r.AddParameter("username", sa.Login);
                r.AddParameter("password", sa.Password);
            }

            if (!string.IsNullOrEmpty(sa.Cookies))
                r.AddParameter("cookie", sa.Cookies);

            r.AddParameter("repeat_config", 0); //allow username/password duplicates
            if (sa.Proxy.Type == "socks") sa.Proxy.Type = "socks5";
            r.AddParameter("proxytype", sa.Proxy.Type);
            r.AddParameter("proxy", $"{sa.Proxy.Address}:{sa.Proxy.Port}:{sa.Proxy.Login}:{sa.Proxy.Password}");

            dynamic fp = new JObject();
            fp.automatic_timezone = "1";
            fp.webrtc = "disabled";
            fp.hardware_concurrency = _cpu.GetRandomEntryFromList();
            fp.device_memory = _memory.GetRandomEntryFromList();
            fp.fonts = string.Join(",", FontsHelper.GetRandomFonts(StaticRandom.Instance.Next(70, 95)));
            fp.screen_resolution = "random";
            fp.canvas = "0"; //real
            fp.client_rects = "1"; //noise
            fp.webgl_image = "1"; //noise
            fp.webgl = "2"; //custom
            fp.webgl_config = new JObject();
            fp.webgl_config.unmasked_vendor = vendor;
            fp.webgl_config.unmasked_renderer = renderer;
            fp.audio = "1"; //add noise
            fp.media_devices = "1"; //fake
            fp.device_name_switch = "2"; //mask
            fp.device_name = sa.Name; //mask
            fp.ua = ua;
            fp.scan_port_type = 1; //protect
            r.AddParameter("fingerprint_config", fp.ToString());
            json = await ExecuteRequestAsync<JObject>(r);

            if (json.code == "8619")
                throw new Exception("This account already exists in AdsPower!");
            if (json.code == "4006")
                throw new Exception("You are logged in AdsPower browser, logout first and run this program again!");
            //if (json.code == "8616")
            //    throw new Exception("You exceeded the number of available profiles!");
            string profileId = json?.data?.id;
            if (profileId == null)
                throw new Exception(json.ToString());
            return profileId;
        }

        protected override Task<bool> ImportCookiesAsync(string profileId, string cookies)
        {
            return Task.FromResult(true);
        }

        protected override async Task<bool> SaveItemToNoteAsync(string profileId, SocialAccount fa)
        {
            var r = new RestRequest("fbcc/user/update-user-info", Method.POST);
            r.AddParameter("fbcc_user_id", profileId);
            r.AddParameter("login_user_comment", fa.ToString(false, false));
            var json = await ExecuteRequestAsync<JObject>(r);
            return json["msg"]?.ToString() == "Success";
        }

        private async Task<T> ExecuteRequestAsync<T>(RestRequest r, string url = "https://api.adspower.net")
        {
            var rc = new RestClient(url);
            if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_cpl))
                (_token, _cpl) = await GetAdspowerApiTokensAsync();
            r.AddCookie("mix_auth_token", _token);
            r.AddHeader("Cpl", _cpl);
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

        private async Task<T> ExecuteLocalRequestAsync<T>(RestRequest r, string url = "http://127.0.0.1:20725")
        {
            var rc = new RestClient(url);
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

        private async Task<(string token, string cpl)> GetAdspowerApiTokensAsync()
        {
            (var login, var password) = GetLoginAndPassword();
            var r = new RestRequest("api/getUniqueId", Method.GET);
            var res = await ExecuteLocalRequestAsync<JObject>(r);
            var uniqueId = res?["data"]?["unique_id"]?.ToString();
            if (uniqueId == null)
                throw new Exception($"Couldn't get UniqueId for AdsPower. Check, that your browser is running!Error:{res}");

            r = new RestRequest("sys/user/passport/login", Method.POST);
            r.AddHeader("Origin", "https://app.adspower.net");
            r.AddHeader("Sec-Fetch-Site", "same-site");
            r.AddHeader("Sec-Fetch-Mode", "cors");
            r.AddHeader("Sec-Fetch-Dest", "empty");
            r.AddHeader("Referer", "https://app.adspower.net/login");
            r.AddHeader("Accept-Encoding", "gzip, deflate, br");
            r.AddParameter("login_name", login);
            r.AddParameter("password", MD5Helper.CreateMD5(password).ToLowerInvariant());
            r.AddParameter("remember", "1");
            r.AddParameter("language", "en-US");
            r.AddParameter("unique_id", uniqueId);
            var rc = new RestClient("https://api.adspower.net");
            rc.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) adspower/3.9.24 Chrome/87.0.4280.141 Electron/11.3.0 Safari/537.36";

            var resp = await rc.ExecuteAsync(r, new CancellationToken());
            var json = JObject.Parse(resp.Content);
            return (resp.Cookies[0].Value, json["data"]["cpl"].ToString());
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
                Console.Write("Enter your Adspower login:");
                var login = Console.ReadLine();
                Console.Write("Enter your Adspower password:");
                var password = Console.ReadLine();
                return (login, password);
            }
        }
    }
}
