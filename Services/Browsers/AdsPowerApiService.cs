using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Interfaces;

namespace YWB.AntidetectAccountParser.Services.Browsers
{
    public class AdsPowerApiService : AbstractAntidetectApiService
    {
        public AdsPowerApiService(IAccountsParser parser, IProxyProvider proxyProvider) : base(parser, proxyProvider)
        {
        }

        protected override Task<List<(string pName, string pId)>> GetProfilesAsync(List<FacebookAccount> accounts) => throw new NotImplementedException();
        protected override Task ImportCookiesAsync(string profileId, string cookies) => throw new NotImplementedException();
        protected override Task<bool> SaveItemToNoteAsync(string profileId, FacebookAccount fa) => throw new NotImplementedException();
    }
}
