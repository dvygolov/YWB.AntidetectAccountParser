using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Services.Actions
{
    public class ActionsFacade<T> where T:SocialAccount
    {
        public List<AccountAction<T>> AccountActions { get; set; }
        public T Account { get; set; }
    }
}
