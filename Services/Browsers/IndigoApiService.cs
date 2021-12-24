using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Model.Accounts;
using YWB.AntidetectAccountParser.Model.Indigo;

namespace YWB.AntidetectAccountParser.Services.Browsers
{
    public class IndigoApiService : AbstractAntidetectApiService
    {
        private string _token;
        private IndigoPlanSettings _ips;
        private Dictionary<string, IndigoProfilesGroup> _allGroups;
        private ConcurrentDictionary<string, IndigoProfileSettings> _profileSettings = new ConcurrentDictionary<string, IndigoProfileSettings>();
        protected override string FileName { get; set; }

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

        public Dictionary<string, IndigoProfilesGroup> AllGroups
        {
            get
            {
                if (_allGroups == null)
                    _allGroups = GetAllGroupsAsync().Result;
                return _allGroups;
            }
        }


        public async Task<List<IndigoProfile>> GetAllProfilesByGroupAsync(string groupName)
        {
            try
            {
                if (!AllGroups.ContainsKey(groupName)) return null;
                var groupId = AllGroups[groupName];
                var allProfiles = await GetAllProfilesAsync();
                var bProfiles = allProfiles[groupId.Sid];
                return bProfiles.OrderBy(b => b.Name).ToList();
            }
            catch (Exception e)
            {
                AddToLog(e, $"Error while trying to get profiles from group {groupName}!");
                return null;
            }
        }

        private async Task<IndigoProfileSettings> GetProfileSettingsAsync(string profileId)
        {
            if (_profileSettings.ContainsKey(profileId)) return _profileSettings[profileId];
            var r = new RestRequest($"clb/p/{profileId}", Method.POST);
            r.AddJsonBody($@"{{""sid"":""00000000-0000-0000-0000-000000000000"",""name"":"""",""browserType"":5,""osType"":""win"",""maskWebRtc"":true,""webrtcPubIpFillOnStart"":true,""webRtcType"":1,""geoFillOnStart"":true,""tzFillOnStart"":true,""geoPermitType"":1,""canvasDefType"":0,""useCanvasNoise"":false,""useGeoSpoofing"":true,""maskMediaDevices"":true,""mediaDevicesVideoInputs"":1,""mediaDevicesAudioInputs"":2,""mediaDevicesAudioOutputs"":1,""storeExtensions"":false,""storeLs"":false,""disablePlugins"":true,""disableFlashPlugin"":true,""forbidConcurrentExecution"":true,""maskFonts"":true,""maskFontGlyphs"":true,""googleServices"":false,""groupId"":""00000000-0000-0000-0000-000000000000"",""offlineProfile"":false,""localPortsProtection"":true,""localPortsExclude"":[],""container"":{{""navigator"":{{""langHdr"":""en-US,en;q=0.9""}},""scrWidth"":1920,""scrHeight"":1200}},""editPermissionType"":1,""storeBookmarks"":false,""storeHistory"":false,""storePasswords"":false,""storeServiceWorkerCache"":false}}");
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

        public async Task<Dictionary<string, List<IndigoProfile>>> GetAllProfilesAsync()
        {
            var r = new RestRequest($"clb/rest/v1/t/{Ips.Uid}/m/{Ips.Uid}/p/");
            var profiles = await ExecuteRequestAsync<IndigoProfile[]>(r);
            return profiles.GroupBy(p => p.Group).ToDictionary(g => g.Key, g => g.Select(p => p).ToList());
        }

        public async Task<Dictionary<string, IndigoProfilesGroup>> GetAllGroupsAsync()
        {
            var r = new RestRequest($"clb/rest/v1/t/{Ips.Uid}/m/{Ips.Uid}/g/");
            var groups = await ExecuteRequestAsync<IndigoProfilesGroup[]>(r);
            return groups.Where(g => !string.IsNullOrEmpty(g.Name)).ToDictionary(g => g.Name, g => g);
        }

        public async Task<IndigoPlanSettings> GetCurrentPlanSettingsAsync()
        {
            var r = new RestRequest("rest/v1/plans/current");
            return await ExecuteRequestAsync<IndigoPlanSettings>(r);
        }

        protected override async Task<List<(string pName, string pId)>> CreateOrChooseProfilesAsync(IList<SocialAccount> accounts)
        {
            var groups = AllGroups.OrderBy(g => g.Key);
            Console.WriteLine("Choose group:");
            var selected = SelectHelper.Select(groups, g => g.Key);
            var createNew = YesNoSelector.ReadAnswerEqualsYes("Should I create new profiles? If not, then you'll choose from existing.");
            List<IndigoProfile> selectedProfiles = null;
            if (createNew)
            {
                Console.WriteLine("Choose operating system:");
                var os = SelectHelper.Select(new[] { "win", "mac" });
                var res = new List<(string, string)>();
                for (int i = 0; i < accounts.Count; i++)
                {
                    Console.WriteLine($"Creating profile {accounts[i].Name}...");
                    var pId = await CreateNewProfileAsync(accounts[i].Name, os, selected.Value.Sid, accounts[i].Proxy);
                    Console.WriteLine($"Profile with ID={pId} created!");
                    res.Add((accounts[i].Name, pId));
                }
                return res;
            }
            else
            {
                var allProfiles = await GetAllProfilesByGroupAsync(selected.Key);
                selectedProfiles = SelectHelper.SelectMultiple(allProfiles, p => p.Name);
                return selectedProfiles.Select(p => (p.Name, p.Uuid)).ToList();
            }
        }

        protected override async Task ImportCookiesAsync(string profileId, string cookies)
        {
            var r = new RestRequest($"api/v1/profile/cookies/import/webext?profileId={profileId}", Method.POST);
            r.AddParameter("text/plain", cookies, ParameterType.RequestBody);
            dynamic json = await ExecuteLocalRequestAsync<JObject>(r);

            if (json != null && json.status == "OK")
                Console.WriteLine("Cookies imported! Adding all data to note...");
            else
            {
                switch (json.message)
                {
                    case "Can't enable Google services":
                    {
                        Console.WriteLine("Couldn't enable Google services, switching them off...");
                        var settings = await GetProfileSettingsAsync(profileId);
                        settings.googleServices = false;
                        settings.loadCustomExtensions = false;
                        await SaveProfileSettingsAsync(settings);

                        dynamic json2 = await ExecuteLocalRequestAsync<JObject>(r);
                        if (json2 != null && json2.status == "OK")
                            Console.WriteLine("Cookies imported! Adding all data to note...");
                        else
                            Console.WriteLine($"Cookies were NOT imported!!!{json} Adding all data to note...");
                        break;
                    }
                }
            }
        }

        public async Task<string> CreateNewProfileAsync(string pName, string os, string groupId, Proxy p)
        {
            var vInputs = StaticRandom.Instance.Next(0, 1);
            var aInputs = StaticRandom.Instance.Next(0, 4);
            var aOutputs = StaticRandom.Instance.Next(0, 4);

            var r = new RestRequest("api/v2/profile", Method.POST);
            var param = @$"{{""name"":""{pName}"",""group"":""{groupId}"",""os"":""{os}"",""browser"":""mimic"",""googleServices"":true,""mediaDevices"":{{""mode"":""FAKE"",""videoInputs"":""{vInputs}"",""audioInputs"":""{aInputs}"",""audioOutputs"":""{aOutputs}""}},""storage"":{{""local"":true,""extensions"":true,""bookmarks"":false,""history"":false,""passwords"":false}},""canvas"":{{""mode"":""REAL""}},""navigator"":{{""language"":""en-US,en;q=0.9,ru-RU;q=0.8""}},""audioContext"":{{""mode"":""NOISE""}},""webGL"":{{""mode"":""NOISE""}},""webGLMetadata"":{{""mode"":""MASK""}},""network"":{{""proxy"":{{""type"":""{p.Type.ToUpper()}"",""host"":""{p.Address}"",""port"":""{p.Port}"",""username"":""{p.Login}"",""password"":""{p.Password}""}}}},""extensions"":{{""enable"":true,""names"":""""}}}}";
            r.AddParameter("application/json", param, ParameterType.RequestBody);
            var res = await ExecuteLocalRequestAsync<JObject>(r);
            if (res["uuid"] == null)
                throw new Exception($"Can't create browser profile! Error:{res}");
            return res["uuid"].ToString();
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
                Console.WriteLine($"Error while trying to save profile's note:{resp["message"]}");
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
                Console.WriteLine($"Error deserializing {resp.Content} to {typeof(T)}");
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
                Console.WriteLine($"Error deserializing {resp.Content} to {typeof(T)}");
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
            var json = await ExecuteLocalRequestAsync<JObject>(r, false);
            if (json["status"].ToString().ToLowerInvariant() == "ok")
                return json["value"].ToString();
            throw new Exception("Couldn't get Indigo's api token!");
        }

        private void AddToLog(Exception e, string msg) => Console.WriteLine($"{msg} - {e}");
    }
}
