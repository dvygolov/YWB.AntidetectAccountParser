using System;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Model;
using YWB.AntidetectAccountParser.Services.Browsers;

namespace YWB.AntidetectAccountParser
{
    internal class ConsoleFlowFiller
    {
        private AbstractAntidetectApiService _b;

        public ConsoleFlowFiller(AbstractAntidetectApiService selectedBrowser)
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
            var group = await SelectHelper.SelectWithCreateAsync(groups, g => g.Name, _b.AddNewTagGroupAsync, true);
            return new FlowSettings { NamingIndex = sIndex, NamingPrefix = namePrefix, Os = os, Group = group };
        }
    }
}