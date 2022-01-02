using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Services.Actions;

namespace YWB.AntidetectAccountsParser.Services.Archives
{
    public interface IArchiveParser<T> where T : SocialAccount
    {
        List<string> Containers { get; set; }
        T Parse(ActionsFacade<T> actions, string filePath);
    }
}