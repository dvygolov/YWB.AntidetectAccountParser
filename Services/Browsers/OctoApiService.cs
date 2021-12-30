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
        protected override string FileName { get; set; } = "octo.txt";
        private const string ApiUrl = "https://app.octobrowser.net/api/v2/automation/";
        private string _token;
        private string[] _oses = new[] { "win", "mac" };

        private async Task<List<AccountGroup>> GetExistingTagsAsync()
        {
            var r = new RestRequest("tags", Method.GET);
            var json = await ExecuteRequestAsync<JObject>(r);
            return json["data"].Select((dynamic g) => new AccountGroup()
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

        public async Task<string> CreateNewProfileAsync(string pName, string os, Proxy proxy, string tag = null)
        {
            var request = new RestRequest("profiles", Method.POST);
            dynamic p = new JObject();
            p.title = pName;
            p.fingerprint = new JObject();
            p.fingerprint.os = os;
            p.proxy = new JObject();
            p.proxy.type = proxy.Type;
            p.proxy.host = proxy.Address;
            p.proxy.port = int.Parse(proxy.Port);
            p.proxy.login = proxy.Login;
            p.proxy.password = proxy.Password;
            p.tags = new JArray();
            if (tag != null) p.tags.Add(tag);

            request.AddParameter("application/json", p.ToString(), ParameterType.RequestBody);
            dynamic res = await ExecuteRequestAsync<dynamic>(request);
            if (res.success != true) throw new Exception(res.ToString());
            return res.data.uuid;
        }


        protected override async Task<List<(string pName, string pId)>> CreateOrChooseProfilesAsync(IEnumerable<SocialAccount> accounts)
        {
            var profiles = new List<(string, string)>();
            Console.WriteLine("Choose operating system:");
            var os = SelectHelper.Select(_oses);

            var tags = await GetExistingTagsAsync();
            Console.WriteLine("Choose a tag for all of these profiles, if needed:");
            var tag = await SelectHelper.SelectWithCreateAsync(tags, t => t.Name, AddNewTag, true);

            var res = new List<(string, string)>();
            foreach (SocialAccount account in accounts)
            {
                Console.WriteLine($"Creating profile {account.Name}...");
                var pId = await CreateNewProfileAsync(account.Name, os, account.Proxy, tag?.Name);
                Console.WriteLine($"Profile with ID={pId} created!");
                res.Add((account.Name, pId));
            }
            return res;
        }


        protected override async Task ImportCookiesAsync(string profileId, string cookies)
        {
            var request = new RestRequest($"profiles/{profileId}/import_cookies", Method.POST);
            var body = @$"{{""cookies"":{cookies}}}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            dynamic res = await ExecuteRequestAsync<JObject>(request);
            if (res.success != true) throw new Exception(res.ToString());
        }

        protected override async Task<bool> SaveItemToNoteAsync(string profileId, SocialAccount fa)
        {
            var request = new RestRequest($"profiles/{profileId}", Method.PATCH);
            dynamic body = new JObject();
            body.description = fa.ToString(false, false);
            request.AddParameter("application/json", body.ToString(), ParameterType.RequestBody);
            dynamic res = await ExecuteRequestAsync<JObject>(request);
            if (res.success != true) throw new Exception(res.ToString());
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
