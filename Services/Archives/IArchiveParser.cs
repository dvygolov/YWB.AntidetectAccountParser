using System.Collections.Generic;
using YWB.AntidetectAccountParser.Model.Accounts;
using YWB.AntidetectAccountParser.Model.Accounts.Actions;

namespace YWB.AntidetectAccountParser.Services.Archives
{
    public interface IArchiveParser<T> where T : SocialAccount
    {
        List<string> Containers { get; set; }
        T Parse(ActionsFacade<T> actions, string filePath);
    }
}