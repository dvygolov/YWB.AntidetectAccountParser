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

        protected override async Task<string> AddProxyAsync(Proxy p)
        {
            p.Type = p.Type == "socks" ? "socks5" : p.Type;
            var r = new RestRequest("add-proxy", Method.POST);
            r.AddParameter("proxy", $"{p.Address}:{p.Port}:{p.Login}:{p.Password}:{p.Type}");
            var json = await ExecuteRequestAsync<JObject>(r);
        }

        protected override async Task<List<Proxy>> GetExistringProxiesAsync()
        {
            var r = new RestRequest("get-proxy", Method.GET);
            var json = await ExecuteRequestAsync<JObject>(r);
        }

        protected override async Task AddAccountAsync(FacebookAccount acc, string proxyId)
        {
            var r = new RestRequest("add-account", Method.POST);
            r.AddParameter("token", acc.Token);
            r.AddParameter("name", acc.Name);
            r.AddParameter("accept_policy", "on");
            r.AddParameter("disable_notifications", "on");
            var json = await ExecuteRequestAsync<JObject>(r);
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
