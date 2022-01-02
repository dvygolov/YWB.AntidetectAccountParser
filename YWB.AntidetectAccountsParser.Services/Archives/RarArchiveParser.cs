using SharpCompress.Archives.Rar;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Services.Actions;

namespace YWB.AntidetectAccountsParser.Services.Archives
{
    public class RarArchiveParser<T>:IArchiveParser<T> where T:SocialAccount
    {
        public List<string> Containers { get; set; }

        public RarArchiveParser(List<string> archives) => Containers = archives;

        public T Parse(ActionsFacade<T> af, string filePath) 
        {
            using (var archive = RarArchive.Open(filePath))
            {
                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    foreach (var a in af.AccountActions)
                    {
                        if (a.Condition(entry.Key.ToLowerInvariant()))
                        {
                            Console.WriteLine($"{a.Message}{entry.Key}");
                            using (var s = entry.OpenEntryStream())
                            {
                                a.Action(s,af.Account);
                            }
                        }
                    }
                }
            }
            return af.Account;
        }

    }
}
