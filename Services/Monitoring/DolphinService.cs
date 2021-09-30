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
    public class DolphinService : AbstractMonitoringService
    {
        private const string FileName = "dolphin.txt";

        protected override async Task<string> AddProxyAsync(Proxy p)
        {
            var r = new RestRequest("proxy/add", Method.POST);
            var pJson = @$"{{ ""proxy"": [
                              {{
                                ""name"": ""{DateTime.Now.ToString("G")}"",
                                ""host"": ""{p.Address}"",
                                ""port"": ""{p.Port}"",
                                ""type"": ""{p.Type}"",
                                ""login"": ""{p.Login}"",
                                ""password"": ""{p.Password}""
                              }}]}}";
            r.AddJsonBody(pJson);
            var json = await ExecuteRequestAsync<JObject>(r);
            return json["data"]["proxy_id"].ToString();
        }

        protected override async Task AddAccountAsync(FacebookAccount acc, string proxyId)
        {
            var r = new RestRequest("accounts/add", Method.POST);
            var u = $@"
                    {{
                        ""name"": ""{acc.Name}"",
                        ""access_token"": ""{acc.Token}"",
                        ""tags"": [],
                        ""proxy"": {{
                            ""id"": {proxyId}
                        }}
                    }}";
            r.AddJsonBody(u);
            var json = await ExecuteRequestAsync<JObject>(r);
        }

        protected override void AddAuthorization(RestRequest r)
        {
            r.AddHeader("Authorization", _token);
        }

        protected override async Task SetTokenAndApiUrlAsync()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(dir, FileName);
            if (File.Exists(fullPath))
            {
                var split = (await File.ReadAllTextAsync(fullPath)).Split(':');
                (_apiUrl, _token) = (split[0], split[1]);
                _apiUrl += "/new/";
            }
            else
            {
                Console.Write("Enter your Dolphin domain (WITHOUT HTTP!):");
                _apiUrl = Console.ReadLine();
                _apiUrl += "/new/";
                Console.Write("Enter your Dolphin API token:");
                _token = Console.ReadLine();
            }
        }

    }
}
