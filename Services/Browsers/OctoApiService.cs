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
        private string[] _oses =new[] {"win", "mac"};

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

            dynamic res = await ExecuteRequestAsync<JObject>(r);
            if (res.success!=true) throw new Exception(res.ToString());
            Console.WriteLine("Proxy added!");
            return res.data.uuid;
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

        private Task<AccountGroup> AddNewTag()
        {
            Console.Write("Enter tag name:");
            var tagName = Console.ReadLine();
            return Task.FromResult(new AccountGroup() { Id = "new", Name = tagName });
        }

        public async Task<string> CreateNewProfileAsync(string pName, string os, string proxyId,string tag=null)
        {
            var request = new RestRequest("profiles", Method.POST);
            dynamic p = new JObject();
            p.title = pName;
            p.fingerprint=new JObject();
            p.fingerprint.os = os;
            p.proxy=new JObject();
            p.proxy.uuid= proxyId;
            p.tags=new JArray();
            if (tag!=null) p.tags.Add(tag);

            request.AddParameter("application/json", p.ToString(), ParameterType.RequestBody);
            dynamic res = await ExecuteRequestAsync<dynamic>(request);
            if (res.success!=true) throw new Exception(res.ToString());
            return res.data.uuid;
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

            var tags = await GetExistingTagsAsync();
            Console.WriteLine("Choose a tag for all of these profiles, if needed:");
            var tag=await SelectHelper.SelectWithCreateAsync(tags, t => t.Name, AddNewTag, true);

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
                var pId = await CreateNewProfileAsync(pName, os, proxyIds[accounts[i].Proxy],tag.Name);
                Console.WriteLine($"Profile with ID={pId} created!");
                res.Add((pName, pId));
            }
            return res;
        }


        protected override async Task ImportCookiesAsync(string profileId, string cookies)
        {
            var request = new RestRequest($"profiles/{profileId}/import_cookies", Method.POST);
            var body = @$"{{""cookies"":{cookies}}}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            dynamic res = await ExecuteRequestAsync<JObject>(request);
            if (res.success!=true) throw new Exception(res.ToString());
        }

        protected override async Task<bool> SaveItemToNoteAsync(string profileId, SocialAccount fa)
        {
            var request = new RestRequest($"profiles/{profileId}", Method.PATCH);
            dynamic body = new JObject();
            body.description = fa.ToString(false,false);
            request.AddParameter("application/json", body.ToString(), ParameterType.RequestBody);
            dynamic res = await ExecuteRequestAsync<JObject>(request);
            if (res.success!=true) throw new Exception(res.ToString());
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
