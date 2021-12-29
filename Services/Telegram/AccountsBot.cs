using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YWB.AntidetectAccountParser.Services.Parsers;

namespace YWB.AntidetectAccountParser.Services.Telegram
{
    internal class AccountsBot
    {
        private const string FileName = "bot.txt";
        private TelegramBotClient _bot;

        public AccountsBot()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(dir, FileName);
            _bot = new TelegramBotClient(System.IO.File.ReadAllText(fullPath));
            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };
            _bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);
        }

        public async Task Listen()
        {
            var me = await _bot.GetMeAsync();
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
        }

        async Task HandleUpdateAsync(ITelegramBotClient b, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message) return;
            var m = update.Message;
            switch (m.Type)
            {
                case MessageType.Document:
                    if (!m.Document.FileName.EndsWith(".txt"))
                    {
                        await b.SendTextMessageAsync(m.Chat.Id, "Unknown file type!");
                        break;
                    }

                    var ms=new MemoryStream();
                    await _bot.GetInfoAndDownloadFileAsync(m.Document.FileId, ms);
                    var content=Encoding.UTF8.GetString(ms.ToArray());
                    //var p = new FacebookTextAccountsParser(()=>m.Document.);
                    break;
            }
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
