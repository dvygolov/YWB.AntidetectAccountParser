using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Services.Parsers
{
    public interface IAccountsParser<out A> where A : SocialAccount
    {
        IEnumerable<A> Parse();
    }
}
