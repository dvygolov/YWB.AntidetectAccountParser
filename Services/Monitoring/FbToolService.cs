using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Interfaces;

namespace YWB.AntidetectAccountParser.Services.Monitoring
{
    public class FbToolService : AbstractMonitoringService
    {
        private const string FileName = "fbtool.txt";

        public override async Task AddAccountsAsync(List<FacebookAccount> accounts)
        {
            var r = new RestRequest("get-proxy", Method.GET);
            var json = await ExecuteRequestAsync<JObject>(r);
            foreach (var acc in accounts)
            {
                var p = acc.Proxy;
                p.Type = p.Type == "socks" ? "socks5" : p.Type;
                r = new RestRequest("add-proxy", Method.POST);
                r.AddParameter("proxy", $"{p.Address}:{p.Port}:{p.Login}:{p.Password}:{p.Type}");
                json = await ExecuteRequestAsync<JObject>(r);

                r = new RestRequest("add-account", Method.POST);
                r.AddParameter("token", acc.Token);
                r.AddParameter("name", acc.Name);
                r.AddParameter("accept_policy", "on");
                r.AddParameter("disable_notifications", "on");
                json = await ExecuteRequestAsync<JObject>(r);
            }
        }
        protected override void AddAuthorization(RestRequest r)
        {
            r.AddQueryParameter("key", _token);
        }

        protected override async Task SetTokenAndApiUrlAsync()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(dir, FileName);
            if (File.Exists(fullPath))
            {
                _token = await File.ReadAllTextAsync(fullPath);
            }
            else
            {
                Console.Write("Enter your FbTool API Token:");
                _token = Console.ReadLine();
            }
            _apiUrl = "https://fbtool.pro/api";
        }
    }
}
