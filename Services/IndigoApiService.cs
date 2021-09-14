using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Model.Indigo;

namespace YWB.AntidetectAccountParser.Services
{
    public class IndigoApiService : IAntidetectApiService
    {
        private string _token;
        private IndigoPlanSettings _ips;
        private Dictionary<string, IndigoProfilesGroup> _allGroups;
        private ConcurrentDictionary<string, IndigoProfileSettings> _profileSettings = new ConcurrentDictionary<string, IndigoProfileSettings>();

        public IndigoPlanSettings Ips
        {
            get
            {
                if (_ips == null)
                {
                    _ips = GetCurrentPlanSettingsAsync().Result;
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

        public async Task ImportLogsAsync()
        {
            var proxy = Proxy.Parse(File.ReadAllText("proxy.txt"));
            var accounts = FacebookAccount.AutoParseFromArchives("logs");
            if (accounts.Count == 0)
            {
                Console.WriteLine("Couldn't find valid accounts to import!");
                return;
            }
            else
                Console.WriteLine($"Found {accounts.Count} accounts.");

            var groups = AllGroups.OrderBy(g => g.Key);
            Console.WriteLine("Choose group:");
            var selected = SelectHelper.Select(groups, g => g.Key);

            var os = string.Empty;
            Console.WriteLine("Choose operating system:");
            os = SelectHelper.Select(new[] { "win", "mac" });

            for (int i = 0; i < accounts.Count; i++)
            {
                if (accounts[i].AllCookies.Count == 0)
                {
                    string pId = string.Empty;
                    string pName = $"PasswordOnly_{accounts[i].Name}";
                    Console.WriteLine($"Creating profile {pName}...");
                    pId = await CreateNewProfileAsync($"{pName}", os, selected.Value.Sid, proxy);
                    Console.WriteLine($"Profile with ID={pId} created!");
                    await SaveItemToNoteAsync(pId, accounts[i].ToString(), true);
                    Console.WriteLine("Note saved!");
                    continue;
                }

                for (int j = 0; j < accounts[i].AllCookies.Count; j++)
                {
                    string pId = string.Empty;
                    string pName = $"{accounts[i].Name}_{j + 1}";
                    Console.WriteLine($"Creating profile {pName}...");
                    pId = await CreateNewProfileAsync($"{pName}", os, selected.Value.Sid, proxy);
                    Console.WriteLine($"Profile with ID={pId} created!");
                    Console.WriteLine($"Importing cookies to account {accounts[i].Name}...");

                    if (CookieHelper.AreCookiesInBase64(accounts[i].Cookies))
                    {
                        accounts[i].Cookies = Encoding.UTF8.GetString(Convert.FromBase64String(accounts[i].Cookies));
                    }
                    var json = await ImportCookiesAsync(pId, accounts[i].Cookies);
                    if (json != null && json["status"].ToString() == "OK")
                        Console.WriteLine("Cookies imported! Saving all data to profile's note...");
                    else
                    {
                        if (json["message"].ToString() == "Can't enable Google services")
                        {
                            Console.WriteLine("Couldn't swith on Google services, switching them off...");
                            var settings = await GetProfileSettingsAsync(pId);
                            settings.googleServices = false;
                            settings.loadCustomExtensions = false;
                            await SaveProfileSettingsAsync(settings);

                            json = await ImportCookiesAsync(pId, accounts[i].Cookies);
                            if (json != null && json["status"].ToString() == "OK")
                                Console.WriteLine("Cookies imported! Saving all data to profile's note...");
                            else
                                Console.WriteLine($"Cookies NOT imported!!!{json} Adding all data to profile's note...");
                        }
                        else
                        {
                            Console.WriteLine($"Couldnt' import all cookies:{json}, trying to import only Facebook's...");
                            json = await ImportCookiesAsync(pId, CookieHelper.GetFacebookCookies(accounts[i].Cookies));
                            if (json != null && json["status"].ToString() == "OK")
                                Console.WriteLine("Facebook cookies imported! Adding all data to profile's note...");
                            else
                                Console.WriteLine($"Cookies NOT imported!!!{json} Adding all data to profile's note...");
                        }
                    }

                    await SaveItemToNoteAsync(pId, accounts[i].ToString(), true);
                    Console.WriteLine("Note saved!");
                }
            }
        }


        public async Task ImportAccountsAsync()
        {
            var proxy = Proxy.Parse(File.ReadAllText("proxy.txt"));
            var accounts = FacebookAccount.AutoParse(File.ReadAllText("accounts.txt"));
            if (accounts.Count == 0)
            {
                Console.WriteLine("Couldn't find any accounts to import! Unknown format or empty accounts.txt file!");
                return;
            }
            else
                Console.WriteLine($"Found {accounts.Count} accounts.");

            var groups = AllGroups.OrderBy(g => g.Key);
            Console.WriteLine("Choose group:");
            var selected = SelectHelper.Select(groups, g => g.Key);
            var createNew = YesNoSelector.ReadAnswerEqualsYes("Should I create new profiles? If not, then you'll choose from existing.");
            var namePrefix = string.Empty;
            var os = string.Empty;
            List<IndigoProfile> selectedProfiles = null;
            if (createNew)
            {
                Console.Write("Enter profile name prefix:");
                namePrefix = Console.ReadLine();
                Console.WriteLine("Choose operating system:");
                os = SelectHelper.Select(new[] { "win", "mac" });
            }
            else
            {
                var allProfiles = await GetAllProfilesByGroupAsync(selected.Key);
                selectedProfiles = SelectHelper.SelectMultiple(allProfiles, p => p.Name);
            }

            for (int i = 0; i < accounts.Count; i++)
            {
                string pId = string.Empty;
                string pName = string.Empty;
                if (createNew)
                {
                    pName = $"{namePrefix}{i}";
                    Console.WriteLine($"Creating profile {pName}...");
                    pId = await CreateNewProfileAsync($"{pName}", os, selected.Value.Sid, proxy);
                    Console.WriteLine($"Profile with ID={pId} created!");
                }
                else
                {
                    pId = selectedProfiles[i].Sid;
                    pName = selectedProfiles[i].Name;
                }

                Console.WriteLine($"Importing {accounts[i].Login} account's cookies to {pName} profile...");

                if (CookieHelper.AreCookiesInBase64(accounts[i].Cookies))
                {
                    accounts[i].Cookies = Encoding.UTF8.GetString(Convert.FromBase64String(accounts[i].Cookies));
                }
                var json = await ImportCookiesAsync(pId, accounts[i].Cookies);
                if (json != null && json["status"].ToString() == "OK")
                    Console.WriteLine("Cookies imported! Adding all data to note...");
                else
                {
                    if (json["message"].ToString() == "Can't enable Google services")
                    {
                        Console.WriteLine("Couldn't enable Google services, switching them off...");
                        var settings = await GetProfileSettingsAsync(pId);
                        settings.googleServices = false;
                        settings.loadCustomExtensions = false;
                        await SaveProfileSettingsAsync(settings);

                        json = await ImportCookiesAsync(pId, accounts[i].Cookies);
                        if (json != null && json["status"].ToString() == "OK")
                            Console.WriteLine("Cookies imported! Adding all data to note...");
                        else
                            Console.WriteLine($"Cookies were NOT imported!!!{json} Adding all data to note...");
                    }
                    else
                    {
                        Console.WriteLine($"Couldn't import all cookies:{json}, trying to import only Facebook...");
                        json = await ImportCookiesAsync(pId, CookieHelper.GetFacebookCookies(accounts[i].Cookies));
                        if (json != null && json["status"].ToString() == "OK")
                            Console.WriteLine("Facebook cookies imported! Adding all data to note...");
                        else
                            Console.WriteLine($"Facebook cookies were NOT imported!!!{json} Adding all data to note...");
                    }
                }

                await SaveItemToNoteAsync(pId, accounts[i].ToString(), true);
                Console.WriteLine("Note saved!");
            }
        }

        private async Task<JObject> ImportCookiesAsync(string profileId, string cookies)
        {
            var r = new RestRequest($"api/v1/profile/cookies/import/webext?profileId={profileId}", Method.POST);
            r.AddParameter("text/plain", cookies, ParameterType.RequestBody);
            return await ExecuteLocalRequestAsync<JObject>(r);
        }

        public async Task<string> CreateNewProfileAsync(string pName, string os, string groupId, Proxy p)
        {
            var r = new RestRequest("api/v2/profile", Method.POST);
            var param = @$"{{""name"":""{pName}"",""group"":""{groupId}"",""os"":""{os}"",""browser"":""mimic"",""googleServices"":true,""mediaDevices"":{{""mode"":""FAKE"",""videoInputs"":""1"",""audioInputs"":""2"",""audioOutputs"":""3""}},""storage"":{{""local"":true,""extensions"":true,""bookmarks"":false,""history"":false,""passwords"":false}},""canvas"":{{""mode"":""REAL""}},""navigator"":{{""language"":""en-US,en;q=0.9,ru-RU;q=0.8""}},""audioContext"":{{""mode"":""NOISE""}},""webGL"":{{""mode"":""NOISE""}},""webGLMetadata"":{{""mode"":""MASK""}},""network"":{{""proxy"":{{""type"":""{p.Type.ToUpper()}"",""host"":""{p.Address}"",""port"":""{p.Port}"",""username"":""{p.Login}"",""password"":""{p.Password}""}}}},""extensions"":{{""enable"":true,""names"":""""}}}}";
            r.AddParameter("application/json", param, ParameterType.RequestBody);
            var res = await ExecuteLocalRequestAsync<JObject>(r);
            return res["uuid"].ToString();
        }

        public async Task<bool> SaveItemToNoteAsync(string profileId, string item, bool replace = false)
        {
            var j = await GetProfileSettingsAsync(profileId);
            if (j == null) { return false; }

            var note = replace ? "" : j.notes;
            note += $" {Environment.NewLine}{item}";
            var r = new RestRequest($"accpmp/rest/ui/v1/profile/{profileId}/note", Method.POST);
            r.AddParameter("text/plain", note, ParameterType.RequestBody);
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
                res= JsonConvert.DeserializeObject<T>(resp.Content);
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
            var rc = new RestClient(url);
            if (addToken)
            {
                if (string.IsNullOrEmpty(_token))
                    _token = await GetIndigoApiTokenAsync();
                r.AddHeader("token", _token);
            }
            var resp = await rc.ExecuteAsync(r, new CancellationToken());
            T res = default(T);
            try 
            {	        
                res= JsonConvert.DeserializeObject<T>(resp.Content);
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
