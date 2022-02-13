using Microsoft.Extensions.Logging;
using System.IO.Compression;
using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Model.Actions;

namespace YWB.AntidetectAccountsParser.Services.Archives
{
    public class ZipArchiveParser<T>:IArchiveParser<T> where T:SocialAccount
    {
        private readonly ILoggerFactory _lf;

        public List<string> Containers { get; set; }

        public ZipArchiveParser(List<string> archives, ILoggerFactory lf)=> (Containers,_lf) = (archives,lf);

        public T Parse(ActionsFacade<T> af, string filePath)
        {
            var l = _lf.CreateLogger<ZipArchiveParser<T>>();
            using (var archive = ZipFile.OpenRead(filePath))
            {
                foreach (var entry in archive.Entries)
                {
                    foreach (var a in af.AccountActions)
                    {
                        if (a.Condition(entry.FullName.ToLowerInvariant()))
                        {
                            l.LogInformation($"{a.Message}{entry.FullName}");
                            using (var s = entry.Open())
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
