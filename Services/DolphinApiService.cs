using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Model.Indigo;
using YWB.AntidetectAccountParser.Services.Interfaces;

namespace YWB.AntidetectAccountParser.Services
{
    public class DolphinApiService : AbstractAntidetectApiService
    {
        public DolphinApiService(IAccountsParser parser, IProxyProvider proxyProvider) : base(parser, proxyProvider) { }

        public Dictionary<string, IndigoProfilesGroup> AllGroups => throw new NotImplementedException();

        public Task<string> CreateNewProfileAsync(string pName, string os, string groupId, Proxy p)
        {
            var client = new RestClient("https://anty-api.com/browser_profiles");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddParameter("name", "");
            request.AddParameter("tags[]", "");
            request.AddParameter("tabs", "");
            request.AddParameter("platform", "");
            request.AddParameter("mainWebsite", "");
            request.AddParameter("useragent[mode]", "");
            request.AddParameter("useragent[value]", "");
            request.AddParameter("webrtc[mode]", "");
            request.AddParameter("webrtc[ipAddress]", "");
            request.AddParameter("canvas[mode]", "");
            request.AddParameter("webgl[mode]", "");
            request.AddParameter("webglInfo[mode]", "");
            request.AddParameter("webglInfo[vendor]", "");
            request.AddParameter("webglInfo[renderer]", "");
            request.AddParameter("notes[icon]", "");
            request.AddParameter("notes[color]", "");
            request.AddParameter("notes[style]", "");
            request.AddParameter("notes[content]", "");
            request.AddParameter("timezone[mode]", "");
            request.AddParameter("timezone[value]", "");
            request.AddParameter("locale[mode]", "");
            request.AddParameter("locale[value]", "");
            request.AddParameter("statusId", "");
            request.AddParameter("geolocation[mode]", "");
            request.AddParameter("geolocation[latitude]", "");
            request.AddParameter("geolocation[longitude]", "");
            request.AddParameter("cpu[mode]", "");
            request.AddParameter("cpu[value]", "");
            request.AddParameter("memory[mode]", "");
            request.AddParameter("memory[value]", "");
            request.AddParameter("doNotTrack", "");
            request.AddParameter("browserType", "");
            request.AddParameter("proxy[id]", "");
            request.AddParameter("proxy[type]", "");
            request.AddParameter("proxy[host]", "");
            request.AddParameter("proxy[port]", "");
            request.AddParameter("proxy[login]", "");
            request.AddParameter("proxy[password]", "");
            request.AddParameter("proxy[name]", "");
            request.AddParameter("proxy[changeIpUrl]", "");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            return Task.FromResult(string.Empty);

        }
        public Task<Dictionary<string, IndigoProfilesGroup>> GetAllGroupsAsync() => throw new NotImplementedException();
        public Task<Dictionary<string, List<IndigoProfile>>> GetAllProfilesAsync() => throw new NotImplementedException();
        public Task<List<IndigoProfile>> GetAllProfilesByGroupAsync(string groupName) => throw new NotImplementedException();
        protected override Task<List<(string pName, string pId)>> GetProfilesAsync(List<FacebookAccount> accounts) => throw new NotImplementedException();
        protected override Task ImportCookiesAsync(string profileId, string cookies) => throw new NotImplementedException();
        protected override Task<bool> SaveItemToNoteAsync(string profileId, string item, bool replace = false) => throw new NotImplementedException();
    }
}
