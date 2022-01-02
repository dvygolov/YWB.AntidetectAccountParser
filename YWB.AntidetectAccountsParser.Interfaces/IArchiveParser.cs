using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Model.Actions;

namespace YWB.AntidetectAccountsParser.Interfaces
{
    public interface IArchiveParser<T> where T : SocialAccount
    {
        List<string> Containers { get; set; }
        T Parse(ActionsFacade<T> actions, string filePath);
    }
}