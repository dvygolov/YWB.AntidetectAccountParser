using System;
using System.Threading.Tasks;
using YWB.IndigoAccountParser.Services;

namespace YWB.IndigoAccountParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Facebook Accounts Parser v1.0 by Yellow Web (https://yellowweb.top)");
            Console.WriteLine("If you like this software, please, donate!");
            Console.WriteLine("WebMoney: Z182653170916");
            Console.WriteLine("Bitcoin: bc1qqv99jasckntqnk0pkjnrjtpwu0yurm0qd0gnqv");
            Console.WriteLine("Ethereum: 0xBC118D3FDE78eE393A154C29A4545c575506ad6B");
            await Task.Delay(5000);
            Console.WriteLine();

            var ias = new IndigoApiService();
            await ias.ImportAccountsAsync();

            Console.WriteLine("All done! Press any key to exit... and don't forget to donate ;-)");
            Console.ReadKey();
        }
    }
}
