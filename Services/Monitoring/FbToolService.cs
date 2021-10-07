using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Interfaces;

namespace YWB.AntidetectAccountParser.Services.Monitoring
{
    public class FbToolService : AbstractMonitoringService
    {
        private const string FileName = "fbtool.txt";

        protected override async Task<List<AccountsGroup>> GetExistingGroupsAsync()
        {
            var r = new RestRequest("get-groups", Method.GET);
            var json = await ExecuteRequestAsync<JObject>(r);
            return json.Children().Select(t => t.First).Where(t => t.HasValues).Select(t => new AccountsGroup()
            {
                Id = t["id"].ToString(),
                Name = t["name"].ToString()
            }).ToList();
        }
        protected override Task<AccountsGroup> AddNewGroupAsync()
        {
            Console.Write("Enter group name:");
            var tagName = Console.ReadLine();
            return Task.FromResult(new AccountsGroup() { Id = "new", Name = tagName });
        }
        protected override async Task<string> AddProxyAsync(Proxy p)
        {
            p.Type = p.Type == "socks" ? "socks5" : p.Type;
            var r = new RestRequest("add-proxy", Method.POST);
            r.AddParameter("proxy", $"{p.Address}:{p.Port}:{p.Login}:{p.Password}:{p.Type}");
            dynamic json = await ExecuteRequestAsync<JObject>(r);
            return "";
        }

        protected override async Task<List<Proxy>> GetExistingProxiesAsync()
        {
            var r = new RestRequest("get-proxy", Method.GET);
            var json = await ExecuteRequestAsync<JObject>(r);
            return json.Children().Select(t => t.First).Where(t => t.HasValues).Select(t =>
            {
                var pStr = t["proxy"].ToString();
                try
                {
                    var s = pStr.Split(":");
                    var p = new Proxy()
                    {
                        Id = t["id"].ToString(),
                        Address = s[0],
                        Port = s[1],
                        Login = s[2],
                        Password = s[3],
                        Type = (t["type"].ToString() == string.Empty ? "http" : (t["type"].ToString() == "https" ? "http" : t["type"].ToString()))
                    };
                    return p;
                }
                catch
                {
                    Console.WriteLine($"Couldn't parse proxy string:{pStr}");
                    return null;
                }
            }).Where(p => p != null).ToList();
        }

        protected override async Task<bool> AddAccountAsync(FacebookAccount acc, AccountsGroup g, string proxyId)
        {
            var r = new RestRequest("add-account", Method.POST);
            r.AddParameter("token", acc.Token);
            r.AddParameter("name", acc.Name);
            if (!string.IsNullOrEmpty(acc.Password))
                r.AddParameter("pass", acc.Password);
            if (!string.IsNullOrEmpty(acc.Cookies))
                r.AddParameter("cookie", acc.Cookies);
            if (!string.IsNullOrEmpty(acc.BmToken))
                r.AddParameter("bm_token", acc.BmToken);
            r.AddParameter("accept_policy", "on");
            r.AddParameter("disable_notifications", "on");
            r.AddParameter("autopublish_fp", "on");
            r.AddParameter("comment_status", "on");
            r.AddParameter("deleteOrHide", 0);
            dynamic json = await ExecuteRequestAsync<JObject>(r);
            if (json.success == false)
            {
                Console.WriteLine($"Couldn't add account {acc.Name} to FbTool. Error:{json.message}");
                return false;
            }
            return true;
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
