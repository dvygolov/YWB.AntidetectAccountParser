using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Services.Actions;
using YWB.AntidetectAccountsParser.Services.Archives;
using YWB.AntidetectAccountsParser.Services.Proxies;

namespace YWB.AntidetectAccountsParser.Services.Parsers
{
    public enum AccountValidity { Valid, PasswordOnly, Invalid }
    public abstract class AbstractArchivesAccountsParser<T> : IAccountsParser<T> where T : SocialAccount
    {
        public IEnumerable<T> Parse()
        {
            var apf = new ArchiveParserFactory<T>();
            var ap = apf.GetArchiveParser();
            List<T> accounts = new List<T>();
            var proxyProvider = new FileProxyProvider();
            var proxies=proxyProvider.Get();

            for (int i = 0; i < ap.Containers.Count; i++)
            {
                string archive = ap.Containers[i];
                var actions = GetActions(archive);
                var acc = ap.Parse(actions, archive);
                var validity = IsValid(acc);
                switch (validity)
                {
                    case AccountValidity.Valid:
                        if (proxies.Count==ap.Containers.Count) acc.Proxy=proxies[i];
                        accounts.Add(acc);
                        break;
                    case AccountValidity.PasswordOnly:
                        if (proxies.Count==ap.Containers.Count) acc.Proxy=proxies[i];
                        acc.Name = $"PasswordOnly_{acc.Name}";
                        accounts.Add(acc);
                        break;
                    case AccountValidity.Invalid:
                        var invalid = Path.Combine(ArchiveParserFactory<T>.Folder, "Invalid");
                        if (!Directory.Exists(invalid)) Directory.CreateDirectory(invalid);
                        File.Move(archive, Path.Combine(invalid, Path.GetFileName(archive)));
                        break;
                }
            }
            return MultiplyCookies(accounts);
        }

        public abstract ActionsFacade<T> GetActions(string filePath);
        public abstract AccountValidity IsValid(T account);
        public abstract IEnumerable<T> MultiplyCookies(IEnumerable<T> accounts);
    }
}
