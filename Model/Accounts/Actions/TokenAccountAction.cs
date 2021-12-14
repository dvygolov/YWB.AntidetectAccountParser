using System;
using System.Text;
using YWB.AntidetectAccountParser.Helpers;

namespace YWB.AntidetectAccountParser.Model.Accounts.Actions
{
    public class TokenAccountAction<T> : AccountAction<T> where T : FacebookAccount
    {
        public TokenAccountAction()
        {
            Condition = (fileName) => fileName.Contains("token");
            Action = (stream,sa) => ExtractToken(stream,sa);
            Message = "Found file with tokens: ";
        }

        private void ExtractToken(System.IO.Stream s,T sa)
        {
            var content = Encoding.UTF8.GetString(s.ReadAllBytes()).Trim();
            if (content.StartsWith("EAAB"))
            {
                sa.Token = content;
                Console.WriteLine("Found Facebook Access Token!");
            }
            if (content.StartsWith("EAAG"))
            {
                sa.BmToken = content;
                Console.WriteLine("Found Business Manager Access Token!");
            }
        }
    }
}
