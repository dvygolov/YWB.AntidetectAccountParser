using System.Collections.Generic;

namespace YWB.AntidetectAccountParser.Model.Accounts.Actions
{
    public class ActionsFacade<T> where T:SocialAccount
    {
        public List<AccountAction<T>> AccountActions { get; set; }
        public T Account { get; set; }
    }
}
