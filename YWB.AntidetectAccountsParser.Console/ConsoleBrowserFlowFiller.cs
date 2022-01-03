using System;
using System.Threading.Tasks;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Services.Browsers;
using YWB.Helpers;

namespace YWB.AntidetectAccountsParser.Terminal
{
    internal class ConsoleBrowserFlowFiller
    {
        private AbstractAntidetectApiService _b;

        public ConsoleBrowserFlowFiller(AbstractAntidetectApiService selectedBrowser)
        {
            _b = selectedBrowser;
        }

        internal async Task<FlowSettings> FillAsync()
        {
            Console.Write("Enter account name prefix:");
            var namePrefix = Console.ReadLine();
            Console.Write("Enter starting index (For example, 1):");
            var sIndex = int.Parse(Console.ReadLine());
            Console.WriteLine("Choose operating system:");
            var oses = _b.GetOSes();
            var os = SelectHelper.Select(oses);
            var groups = await _b.GetExistingGroupsAsync();
            Console.WriteLine("Choose a tag/group for all of your profiles, if needed:");
            var group = await SelectHelper.SelectWithCreateAsync(groups, g => g.Name, async () => 
            {
                Console.Write("Enter new tag/group name:");
                var tagName = Console.ReadLine();
                return await _b.AddNewGroupAsync(tagName);
            }, true);
            return new FlowSettings { NamingIndex = sIndex, NamingPrefix = namePrefix, Os = os, Group = group };
        }
    }
}