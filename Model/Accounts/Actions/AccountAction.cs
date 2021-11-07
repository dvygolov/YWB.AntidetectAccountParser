using System;
using System.IO;

namespace YWB.AntidetectAccountParser.Model.Accounts
{
    public class AccountAction<T> where T:SocialAccount
    {
        public string Message { get; set; }

        public Func<string,bool> Condition { get; set; }

        public Action<Stream,T> Action { get; set; }
    }
}
