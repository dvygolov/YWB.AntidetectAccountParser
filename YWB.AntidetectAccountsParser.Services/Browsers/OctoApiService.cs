using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Reflection;
using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Services.Browsers
{
    public class OctoApiService : AbstractAntidetectApiService
    {
        protected override string FileName { get; set; } = "octo.txt";
        private const string ApiUrl = "https://app.octobrowser.net/api/v2/automation/";
        private string _token;
        private string[] _oses = new[] { "win", "mac" };

        public override List<string> GetOSes() => _oses.ToList();

        public override Task<AccountGroup> AddNewGroupAsync(string groupName)
        {
            return Task.FromResult(new AccountGroup() { Id = "new", Name = groupName });
        }

        public override async Task<List<AccountGroup>> GetExistingGroupsAsync()
        {
            var r = new RestRequest("tags", Method.GET);
            var json = await ExecuteRequestAsync<JObject>(r);
            return json["data"].Select((dynamic g) => new AccountGroup()
            {
                Id = g.uuid,
                Name = g.name
            }).ToList();
        }

        public override async Task<string> CreateNewProfileAsync(SocialAccount acc, string os, AccountGroup group)
        {
            var request = new RestRequest("profiles", Method.POST);
            dynamic p = new JObject();
            p.title = acc.Name;
            p.fingerprint = new JObject();
            p.fingerprint.os = os;
            p.proxy = new JObject();
            p.proxy.type = acc.Proxy.Type;
            p.proxy.host = acc.Proxy.Address;
            p.proxy.port = int.Parse(acc.Proxy.Port);
            p.proxy.login = acc.Proxy.Login;
            p.proxy.password = acc.Proxy.Password;
            p.tags = new JArray();
            if (group != null) p.tags.Add(group.Name);

            request.AddParameter("application/json", p.ToString(), ParameterType.RequestBody);
            dynamic res = await ExecuteRequestAsync<dynamic>(request);
            if (res.success != true) throw new Exception(res.ToString());
            return res.data.uuid;
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
