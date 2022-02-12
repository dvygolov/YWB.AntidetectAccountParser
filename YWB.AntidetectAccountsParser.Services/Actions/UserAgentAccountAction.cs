using System.Text;
using System.Text.RegularExpressions;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Model.Actions;
using YWB.Helpers;

namespace YWB.AntidetectAccountsParser.Services.Actions
{
    public class UserAgentAccountAction<T> : AccountAction<T> where T : SocialAccount
    {
        public UserAgentAccountAction()
        {
            Condition = (fileName) => fileName.ToLowerInvariant().Contains("useragent");
            Action = (stream,sa) => ExtractUserAgent(stream,sa);
            Message = "Found file with useragent: ";
        }

        private void ExtractUserAgent(Stream s,T sa)
        {
            var content = Encoding.UTF8.GetString(s.ReadAllBytes()).Trim();
            var re = new Regex(@"(?<UserAgent>Mozilla.*?Gecko\)\s+(\w+/[\d+\. ]+)+)");
            if(re.IsMatch(content))
            {
                var ua = re.Match(content).Groups["UserAgent"].Value;
                sa.UserAgent = ua;
                Console.WriteLine("Useragent found!");
            }
        }
    }
}
