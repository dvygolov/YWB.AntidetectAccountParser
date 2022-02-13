using Microsoft.Extensions.Logging;
using SharpCompress.Archives.Rar;
using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Model.Actions;

namespace YWB.AntidetectAccountsParser.Services.Archives
{
    public class RarArchiveParser<T>:IArchiveParser<T> where T:SocialAccount
    {
        private readonly ILoggerFactory _lf;

        public List<string> Containers { get; set; }

        public RarArchiveParser(List<string> archives, ILoggerFactory lf) => (Containers,_lf) = (archives,lf);

        public T Parse(ActionsFacade<T> af, string filePath) 
        {
            var l = _lf.CreateLogger<RarArchiveParser<T>>();
            using (var archive = RarArchive.Open(filePath))
            {
                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    foreach (var a in af.AccountActions)
                    {
                        if (a.Condition(entry.Key.ToLowerInvariant()))
                        {
                            l.LogInformation($"{a.Message}{entry.Key}");
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
