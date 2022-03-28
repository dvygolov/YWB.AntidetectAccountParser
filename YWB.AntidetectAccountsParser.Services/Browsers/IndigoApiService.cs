using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Concurrent;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Model.Indigo;
using YWB.Helpers;

namespace YWB.AntidetectAccountsParser.Services.Browsers
{
    public class IndigoApiService : AbstractAntidetectApiService
    {
        private string _token;
        private IndigoPlanSettings _ips;
        private ConcurrentDictionary<string, IndigoProfileSettings> _profileSettings = new ConcurrentDictionary<string, IndigoProfileSettings>();

        public IndigoApiService(string credentials,ILoggerFactory lf) : base(credentials,lf) {}

        public IndigoPlanSettings Ips
        {
            get
            {
                if (_ips == null)
                {
                    _ips = GetCurrentPlanSettingsAsync().Result;
                    if (!_ips.AutomatitonApi)
                        throw new Exception("Your tarification plan does not support automation!");
                    if (_ips.CollaborationMember)
                        throw new Exception("Your should use Indigo's master-account not sub-account to run this software!");
                }
                return _ips;
            }
        }

        public override List<string> GetOSes() => new List<string> { "win", "mac" };

        public override async Task<List<AccountGroup>> GetExistingGroupsAsync()
        {
            var r = new RestRequest($"clb/rest/v1/t/{Ips.Uid}/m/{Ips.Uid}/g/");
            var groups = await ExecuteRequestAsync<IndigoProfilesGroup[]>(r);
            return groups.Select(g => new AccountGroup { Id = g.Sid, Name = g.Name }).ToList();
        }

        public async override Task<AccountGroup> AddNewGroupAsync(string groupName)
        {
            var r = new RestRequest($"clb/u/{Ips.Uid}/g/", Method.POST);
            r.AddJsonBody($@"{{""name"":""{groupName}"",""accessRights"":""[]"",""accessRightsModified"":true}}");
            var group = await ExecuteLocalRequestAsync<IndigoProfilesGroup>(r);
            return new AccountGroup { Id=group.Sid, Name = group.Name };
        }

        private async Task<IndigoProfileSettings> GetProfileSettingsAsync(string profileId)
        {
            if (_profileSettings.ContainsKey(profileId)) return _profileSettings[profileId];
            var r = new RestRequest($"clb/p/{profileId}", Method.POST);
            dynamic s=new JObject();
            s.sid = "00000000-0000-0000-0000-000000000000";
            s.name="";
            s.browserType = 5;
            s.osType = "win";
            s.maskWebRtc = true;
            s.webrtcPubIpFillOnStart=true;
            s.webRtcType = 1;
            s.geoFillOnStart = true;
            s.tzFillOnStart = true;
            s.geoPermitType = 1;
            s.cavasDefType = 0;
            s.useCanvasNoise = false;
            s.useGeoSpoofing = true;
            s.maskMediaDevices = true;
            s.mediaDevicesVideoInputs = 1;
            s.mediaDevicesAudioInputs = 2;
            s.mediaDevicesAudioOutputs = 1;
            s.storeExtensions = false;
            s.storeLs = false;
            s.disablePlugins = true;
            s.disableFlashPlugin=true;
            s.forbidConcurrentExecution = true;
            s.maskFonts=true;
            s.maskFontGlyphs = true;
            s.googleServices = false;
            s.groupId = "00000000-0000-0000-0000-000000000000";
            s.offlineProfile = false;
            s.localPortsProtection = true;
            s.localPortsExclude = new JArray();
            s.container = new JObject();
            s.container.navigator = new JObject();
            s.container.navigator.langHdr = "en-US,en;q=0.9";
            s.container.srcWidth = 1920;
            s.container.srcHeight = 1200;
            s.editPermissionType = 1;
            s.storeBookmarks = false;
            s.storeHistory = true;
            s.storePasswords = true;
            s.storeServiceWorkerCache = false;

            r.AddJsonBody(s.ToString());
            var ips = await ExecuteLocalRequestAsync<IndigoProfileSettings>(r);
            if (ips != null)
                _profileSettings.AddOrUpdate(profileId, _ => ips, (_, _) => ips);
            return ips;
        }

        private async Task SaveProfileSettingsAsync(IndigoProfileSettings ips)
        {
            var r = new RestRequest($"clb/t/{Ips.Uid}/m/{Ips.Uid}/p/save", Method.POST);
            r.RequestFormat = DataFormat.Json;
            r.AddJsonBody(JsonConvert.SerializeObject(ips));
            await ExecuteLocalRequestAsync(r);
            _profileSettings[ips.sid] = ips;
        }

        public async Task<IndigoPlanSettings> GetCurrentPlanSettingsAsync()
        {
            var r = new RestRequest("rest/v1/plans/current");
            return await ExecuteRequestAsync<IndigoPlanSettings>(r);
        }


        public override async Task<string> CreateNewProfileAsync(SocialAccount acc, string os, AccountGroup group)
        {
            var vInputs = StaticRandom.Instance.Next(0, 1);
            var aInputs = StaticRandom.Instance.Next(0, 4);
            var aOutputs = StaticRandom.Instance.Next(0, 4);

            var r = new RestRequest("api/v2/profile", Method.POST);
            dynamic p = new JObject();
            p.name=acc.Name;
            p.group = group.Id;
            p.os=os;
            p.browser = "mimic";
            p.googleServices = false;
            dynamic md=new JObject();
            md.mode = "FAKE";
            md.videoInputs = vInputs;
            md.audioInputs=aInputs;
            md.audioOutputs = aOutputs;
            p.mediaDevices = md;
            dynamic strg=new JObject();
            strg.local = true;
            strg.extensions = true;
            strg.bookmarks = true;
            strg.history = true;
            strg.passwords = true;
            p.storage = strg;
            p.canvas = new JObject();
            p.canvas.mode = "REAL";
            dynamic n = new JObject();
            n.language = "en-US,en;q=0.9,ru-RU;q=0.8";
            if (!string.IsNullOrEmpty(acc.UserAgent))
                n.userAgent = acc.UserAgent;
            p.navigator = n;
            p.audioContext = new JObject();
            p.audioContext.mode = "NOISE";
            p.webGL = new JObject();
            p.webGL.mode = "NOISE";
            p.webGLMetadata = new JObject();
            p.webGLMetadata.mode = "MASK";
            p.network = new JObject();
            dynamic pr = new JObject();
            pr.type = acc.Proxy.Type.ToUpper();
            pr.host = acc.Proxy.Address;
            pr.port = acc.Proxy.Port;
            pr.username = acc.Proxy.Login;
            pr.password = acc.Proxy.Password;
            p.network.proxy = pr;
            dynamic ext = new JObject();
            ext.enable = true;
            ext.names = "";
            p.extensions = ext;

            r.AddParameter("application/json", p.ToString(), ParameterType.RequestBody);
            var res = await ExecuteLocalRequestAsync<JObject>(r);
            if (res["uuid"] == null)
                throw new Exception($"Can't create browser profile! Error:{res}");
            return res["uuid"].ToString();
        }

        protected override async Task ImportCookiesAsync(string profileId, string cookies)
        {
            var r = new RestRequest($"api/v1/profile/cookies/import/webext?profileId={profileId}", Method.POST);
            r.AddParameter("text/plain", cookies, ParameterType.RequestBody);
            dynamic json = await ExecuteLocalRequestAsync<JObject>(r);

            if (json != null && json.status == "OK")
                _logger.LogInformation("Cookies imported! Adding all data to note...");
            else
            {
                switch (json.message)
                {
                    case "Can't enable Google services":
                        {
                            _logger.LogWarning("Couldn't enable Google services, switching them off...");
                            var settings = await GetProfileSettingsAsync(profileId);
                            settings.googleServices = false;
                            settings.loadCustomExtensions = false;
                            await SaveProfileSettingsAsync(settings);

                            dynamic json2 = await ExecuteLocalRequestAsync<JObject>(r);
                            if (json2 != null && json2.status == "OK")
                                _logger.LogInformation("Cookies imported! Adding all data to note...");
                            else
                                _logger.LogWarning($"Cookies were NOT imported!!!{json} Adding all data to note...");
                            break;
                        }
                }
            }
        }


        protected override async Task<bool> SaveItemToNoteAsync(string profileId, SocialAccount fa)
        {
            var j = await GetProfileSettingsAsync(profileId);
            if (j == null) { return false; }

            var r = new RestRequest($"accpmp/rest/ui/v1/profile/{profileId}/note", Method.POST);
            r.AddParameter("text/plain", fa.ToString(), ParameterType.RequestBody);
            var resp = await ExecuteRequestAsync<JObject>(r);
            if (resp["status"].ToString().ToUpperInvariant() == "ERROR")
            {
                _logger.LogError($"Error while trying to save profile's note:{resp["message"]}");
                return false;
            }
            else
            {
                _profileSettings[j.sid] = j;
                return true;
            }
        }

        private async Task<T> ExecuteRequestAsync<T>(
            RestRequest r,
            string url = "https://indigo.multiloginapp.com")
        {
            var rc = new RestClient(url);
            if (string.IsNullOrEmpty(_token))
                _token = await GetIndigoApiTokenAsync();
            r.AddHeader("token", _token);
            rc.UserAgent = "Mozilla/5.0  MultiLoginApp ui client. 5.14.0.29";
            var resp = await rc.ExecuteAsync(r, new CancellationToken());
            T res = default(T);
            try
            {
                res = JsonConvert.DeserializeObject<T>(resp.Content);
            }
            catch (Exception)
            {
                _logger.LogError($"Error deserializing {resp.Content} to {typeof(T)}");
                throw;
            }
            return res;
        }

        private async Task<T> ExecuteLocalRequestAsync<T>(RestRequest r, bool addToken = true)
        {
            string url = $"http://127.0.0.1:35000";
            if (addToken)
            {
                if (string.IsNullOrEmpty(_token))
                    _token = await GetIndigoApiTokenAsync();
                r.AddHeader("token", _token);
            }
            IRestResponse resp;
            int tryCount = 0;
            do
            {
                var rc = new RestClient(url);
                resp = await rc.ExecuteAsync(r, new CancellationToken());
                tryCount++;
                if (resp.StatusCode != System.Net.HttpStatusCode.OK) await Task.Delay(1000);
            }
            while (resp.StatusCode != System.Net.HttpStatusCode.OK && tryCount < 3);

            T res = default(T);
            try
            {
                res = JsonConvert.DeserializeObject<T>(resp.Content);
            }
            catch (Exception)
            {
                _logger.LogError($"Error deserializing {resp.Content} to {typeof(T)}");
                throw;
            }
            return res;
        }

        private async Task ExecuteLocalRequestAsync(RestRequest r)
        {
            string url = $"http://127.0.0.1:35000";
            if (string.IsNullOrEmpty(_token))
                _token = await GetIndigoApiTokenAsync();
            r.AddHeader("token", _token);
            var rc = new RestClient(url);
            var resp = await rc.ExecuteAsync(r, new CancellationToken());
        }

        private async Task<string> GetIndigoApiTokenAsync()
        {
            var r = new RestRequest("/bridge/apiToken", Method.GET);
            dynamic json = await ExecuteLocalRequestAsync<JObject>(r, false);
            if(!json.ContainsKey("status")||!json.ContainsKey("value"))
                throw new Exception($"Couldn't get Indigo's api token: {json}");
            if (json.status.ToString().ToLowerInvariant() == "ok")
                return json.value.ToString();
            throw new Exception($"Couldn't get Indigo's api token: {json}");
        }
    }
}
