using System;
using System.Collections.Concurrent;
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
using YWB.AntidetectAccountParser.Services.Logging;
using YWB.AntidetectAccountParser.Services.Parsers;

namespace YWB.AntidetectAccountParser.Services.Telegram
{
    internal class AccountsBot
    {
        ConcurrentDictionary<string, Dictionary<string, string>> _flows = new ConcurrentDictionary<string, Dictionary<string, string>>();
        private const string FileName = "bot.txt";
        private TelegramBotClient _bot;

        public AccountsBot()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(dir, FileName);
            if (!System.IO.File.Exists(fullPath)) return;
            _bot = new TelegramBotClient(System.IO.File.ReadAllText(fullPath));
            using var cts = new CancellationTokenSource();

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

                    await b.SendTextMessageAsync(m.Chat.Id, "Received file with accounts! Parsing...");
                    var ms = new MemoryStream();
                    await _bot.GetInfoAndDownloadFileAsync(m.Document.FileId, ms);
                    var content = Encoding.UTF8.GetString(ms.ToArray());
                    var logger = new BufferAccountsLogger();
                    var p = new FacebookTextAccountsParser(logger, content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList());
                    p.Parse();
                    await b.SendTextMessageAsync(m.Chat.Id, logger.Flush());
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
