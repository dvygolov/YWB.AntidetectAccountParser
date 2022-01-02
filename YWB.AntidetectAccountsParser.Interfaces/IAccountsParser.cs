using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Interfaces
{
    public interface IAccountsParser<out A> where A : SocialAccount
    {
        IEnumerable<A> Parse();
    }
}
