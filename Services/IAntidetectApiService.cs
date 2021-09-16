using System.Collections.Generic;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Model.Indigo;

namespace YWB.AntidetectAccountParser.Services
{
    public interface IAntidetectApiService
    {
        Dictionary<string, IndigoProfilesGroup> AllGroups { get; }

        Task<string> CreateNewProfileAsync(string pName, string os, string groupId, Proxy p);
        Task<Dictionary<string, IndigoProfilesGroup>> GetAllGroupsAsync();
        Task<Dictionary<string, List<IndigoProfile>>> GetAllProfilesAsync();
        Task<List<IndigoProfile>> GetAllProfilesByGroupAsync(string groupName);
        Task ImportAccountsAsync();
        Task ImportLogsAsync();
        Task<bool> SaveItemToNoteAsync(string profileId, string item, bool replace = false);
    }
}