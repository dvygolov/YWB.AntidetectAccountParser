using System.Reflection;
using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model.Accounts;

namespace YWB.AntidetectAccountsParser.Services.Archives
{
    public class ArchiveParserFactory<T> where T:SocialAccount
    {
        public const string Folder = "logs";
        public IArchiveParser<T> GetArchiveParser()
        {
            var fullDirPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Folder);
            var files = Directory.GetFiles(fullDirPath, "*.zip");
            if (files.Length != 0)
                return new ZipArchiveParser<T>(files.ToList());
            files = Directory.GetFiles(fullDirPath, "*.rar");
            if (files.Length != 0)
                return new RarArchiveParser<T>(files.ToList());
            var dirs=Directory.GetDirectories(fullDirPath);
            if (dirs.Length != 0)
                return new DirParser<T>(dirs.ToList());
            throw new FileNotFoundException("Didn't find any ZIP/RAR archives or Folders to parse!"); }
    }
}
