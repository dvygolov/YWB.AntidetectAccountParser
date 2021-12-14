using System.Collections.Generic;
using System.IO;
using YWB.AntidetectAccountParser.Model.Accounts;
using YWB.AntidetectAccountParser.Model.Accounts.Actions;
using YWB.AntidetectAccountParser.Services.Archives;

namespace YWB.AntidetectAccountParser.Services.Parsers
{
    public enum AccountValidity { Valid, PasswordOnly, Invalid }
    public abstract class AbstractArchivesAccountsParser<T> : IAccountsParser<T> where T : SocialAccount
    {
        public IEnumerable<T> Parse()
        {
            var apf = new ArchiveParserFactory<T>();
            var ap = apf.GetArchiveParser();
            List<T> accounts = new List<T>();
            foreach (var archive in ap.Containers)
            {
                var actions = GetActions(archive);
                var acc = ap.Parse(actions, archive);
                var validity = IsValid(acc);
                switch (validity)
                {
                    case AccountValidity.Valid:
                        accounts.Add(acc);
                        break;
                    case AccountValidity.PasswordOnly:
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
