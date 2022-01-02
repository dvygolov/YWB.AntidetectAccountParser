using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.Helpers;

namespace YWB.AntidetectAccountsParser.Services.Browsers
{
    public class AdsPowerApiService : AbstractAntidetectApiService
    {
        private string _token;
        private string _cpl;
        private List<string> _oses = new List<string> { "Windows", "Mac OS X", "Linux" };
        private List<string> _cpu = new List<string> { "2", "4", "6", "8", "16" };
        private List<string> _memory = new List<string> { "2", "4", "6", "8" };

        public AdsPowerApiService(string credentials) : base(credentials) { }

        public override List<string> GetOSes() => _oses;
        public override Task<List<AccountGroup>> GetExistingGroupsAsync()
        {
            return Task.FromResult(new List<AccountGroup>());
        }

        public override Task<AccountGroup> AddNewGroupAsync(string groupName)
        {
            return Task.FromResult(new AccountGroup { Name = groupName });
        }

        public async override Task<string> CreateNewProfileAsync(SocialAccount acc, string os, AccountGroup group)
        {
            var r = new RestRequest("fbcc/user/rand-get-user-agent", Method.POST);
            r.AddParameter("system", os);
            var json = await ExecuteRequestAsync<JObject>(r);
            string ua = json["data"]["ua"].ToString();

            r = new RestRequest("fbcc/user/random-webgl-config", Method.POST);
            r.AddParameter("ua", ua);
            json = await ExecuteRequestAsync<JObject>(r);
            string renderer = json["data"]["unmasked_renderer"].ToString();
            string vendor = json["data"]["unmasked_vendor"].ToString();

            r = new RestRequest("fbcc/user/single-import-user", Method.POST);
            r.AddParameter("batch_id", "0");
            r.AddParameter("name", acc.Name);
            r.AddParameter("domain_name", acc.Domain);
            if (!string.IsNullOrEmpty(acc.Login) && !string.IsNullOrEmpty(acc.Password))
            {
                r.AddParameter("username", acc.Login);
                r.AddParameter("password", acc.Password);
            }

            if (!string.IsNullOrEmpty(acc.Cookies))
                r.AddParameter("cookie", acc.Cookies);

            r.AddParameter("repeat_config", 0); //allow username/password duplicates
            if (acc.Proxy.Type == "socks") acc.Proxy.Type = "socks5";
            r.AddParameter("proxytype", acc.Proxy.Type);
            r.AddParameter("proxy", $"{acc.Proxy.Address}:{acc.Proxy.Port}:{acc.Proxy.Login}:{acc.Proxy.Password}");

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
            fp.device_name = acc.Name; //mask
            fp.ua = ua;
            fp.scan_port_type = 1; //protect
            r.AddParameter("fingerprint_config", fp.ToString());
            json = await ExecuteRequestAsync<JObject>(r);

            if (json["code"].ToString() == "8619")
                throw new Exception("This account already exists in AdsPower!");

            if (json["code"].ToString() == "4006")
                throw new Exception("You are logged in AdsPower browser, logout first and run this program again!");
            return json["data"]["id"].ToString();
        }

        protected override Task ImportCookiesAsync(string profileId, string cookies)
        {
            return Task.CompletedTask;
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
            var uniqueId = res["data"]?["unique_id"]?.ToString();
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
            var split = _credentials.Split(':');
            return (split[0], split[1]);
        }
    }
}
