using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Interfaces;

namespace YWB.AntidetectAccountParser.Services.Browsers
{
    public class AdsPowerApiService : AbstractAntidetectApiService
    {
        private string _token;
        private string _cpl;
        private const string FileName = "adspower.txt";
        private List<string> _oses = new List<string> { "windows", "macos", "linux" };
        public AdsPowerApiService(IAccountsParser parser, IProxyProvider proxyProvider) : base(parser, proxyProvider)
        {
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

            var res = new List<(string, string)>();
            for (int i = 0; i < accounts.Count; i++)
            {
                var proxyIndex = i < proxies.Count - 1 ? i : i % proxies.Count;
                var p = proxies[proxyIndex];
                var pName = string.IsNullOrEmpty(accounts[i].Name) ? $"{namePrefix}{i}" : accounts[i].Name;
                Console.WriteLine($"Creating profile {pName}...");
                var pId = await CreateNewProfileAsync(pName, os, p);
                Console.WriteLine($"Profile with ID={pId} created!");
                res.Add((pName, pId));
            }
            return res;

        }

        private async Task<string> CreateNewProfileAsync(string pName, string os, Proxy p)
        {
            if (p.Type == "socks") p.Type = "socks5";
            var r = new RestRequest("api/v1/user/create", Method.POST);
            r.AddParameter("name", pName);
            r.AddParameter("domain_name", "facebook.com");
            dynamic ap = new JObject();
            ap.proxy_type = p.Type;
            ap.proxy_host = p.Address;
            ap.proxy_port = p.Port;
            ap.proxy_user = p.Login;
            ap.proxy_password = p.Password;
            r.AddParameter("user_proxy_config", ap.ToString());
            dynamic fp = new JObject();
            fp.automatic_timezone = "1";
            fp.webrtc = "disabled";
            fp.canvas = "0";
            fp.webgl_image = "1";
            r.AddParameter("fingerprint_config", fp.ToString());
            var json=await ExecuteRequestAsync<JObject>(r);
            return string.Empty;
        }

        protected override Task ImportCookiesAsync(string profileId, string cookies) => throw new NotImplementedException();
        protected override Task<bool> SaveItemToNoteAsync(string profileId, FacebookAccount fa) => throw new NotImplementedException();

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
            //r.AddParameter("c_id", "3274d503");
            //r.AddParameter("wr", "5.164.164.223");
            r.AddParameter("unique_id", res["data"]["unique_id"]);
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
