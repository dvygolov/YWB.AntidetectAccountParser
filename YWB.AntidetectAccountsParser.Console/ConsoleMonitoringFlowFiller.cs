using System;
using System.Threading.Tasks;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Services.Monitoring;
using YWB.Helpers;

namespace YWB.AntidetectAccountParser
{
    internal class ConsoleMonitoringFlowFiller
    {
        private AbstractMonitoringService _m;

        public ConsoleMonitoringFlowFiller(AbstractMonitoringService selectedMonitoringService)
        {
            _m = selectedMonitoringService;
        }

        internal async Task<FlowSettings> FillAsync()
        {
            Console.Write("Enter account name prefix:");
            var namePrefix = Console.ReadLine();
            Console.Write("Enter starting index (For example, 1):");
            var sIndex = int.Parse(Console.ReadLine());
            var groups = await _m.GetExistingGroupsAsync();
            Console.WriteLine("Choose a tag/group for all of your profiles, if needed:");
            var group = await SelectHelper.SelectWithCreateAsync(groups, g => g.Name, async () => 
            {
                Console.Write("Enter new tag/group name:");
                var tagName = Console.ReadLine();
                return await _m.AddNewGroupAsync(tagName);
            }, true);
            return new FlowSettings { NamingIndex = sIndex, NamingPrefix = namePrefix, Group = group };
        }
    }
}