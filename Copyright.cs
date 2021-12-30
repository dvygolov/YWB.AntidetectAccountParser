using System;
using System.Threading.Tasks;

namespace YWB.AntidetectAccountParser
{
    internal class Copyright
    {
        public static async Task ShowAsync(bool isBot)
        {
            if (isBot)
            Console.WriteLine(@"            Antidetect Accounts Parser Telegram Bot v6.0 ");
            else
            Console.WriteLine(@"                Antidetect Accounts Parser v6.0 ");
            Console.WriteLine(@"   _            __     __  _ _             __          __  _     ");
            Console.WriteLine(@"  | |           \ \   / / | | |            \ \        / / | |    ");
            Console.WriteLine(@"  | |__  _   _   \ \_/ /__| | | _____      _\ \  /\  / /__| |__  ");
            Console.WriteLine(@"  | '_ \| | | |   \   / _ \ | |/ _ \ \ /\ / /\ \/  \/ / _ \ '_ \ ");
            Console.WriteLine(@"  | |_) | |_| |    | |  __/ | | (_) \ V  V /  \  /\  /  __/ |_) |");
            Console.WriteLine(@"  |_.__/ \__, |    |_|\___|_|_|\___/ \_/\_/    \/  \/ \___|_.__/ ");
            Console.WriteLine(@"          __/ |                                                  ");
            Console.WriteLine(@"         |___/                  https://yellowweb.top            ");
            Console.WriteLine();
            Console.WriteLine("If you like this software, please, donate!");
            Console.WriteLine("WebMoney: Z182653170916");
            Console.WriteLine("Bitcoin: bc1qqv99jasckntqnk0pkjnrjtpwu0yurm0qd0gnqv");
            Console.WriteLine("Ethereum: 0xBC118D3FDE78eE393A154C29A4545c575506ad6B");
            await Task.Delay(3000);
            Console.WriteLine();
        }
    }
}
