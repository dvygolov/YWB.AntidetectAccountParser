using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YWB.AntidetectAccountParser.Helpers;
using YWB.AntidetectAccountParser.Services;

namespace YWB.AntidetectAccountParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Antidetect Accounts Parser v2.0 by Yellow Web (https://yellowweb.top)");
            Console.WriteLine("If you like this software, please, donate!");
            Console.WriteLine("WebMoney: Z182653170916");
            Console.WriteLine("Bitcoin: bc1qqv99jasckntqnk0pkjnrjtpwu0yurm0qd0gnqv");
            Console.WriteLine("Ethereum: 0xBC118D3FDE78eE393A154C29A4545c575506ad6B");
            await Task.Delay(5000);
            Console.WriteLine();

            var ias = new IndigoApiService();
            Console.WriteLine("What do you want to parse?");
            var actions = new List<string> { "Accounts from text file", "Accounts from ZIP files" };
            var selected = SelectHelper.Select(actions);
            switch (actions.IndexOf(selected))
            {
                case 0:
                    await ias.ImportAccountsAsync();
                    break;
                case 1:
                    await ias.ImportLogsAsync();
                    break;
            }


            Console.WriteLine("All done! Press any key to exit... and don't forget to donate ;-)");
            Console.ReadKey();
        }
    }
}
