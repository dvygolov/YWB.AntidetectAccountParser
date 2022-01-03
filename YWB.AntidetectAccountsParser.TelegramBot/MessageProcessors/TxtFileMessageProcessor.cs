using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YWB.AntidetectAccountsParser.Services.Logging;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class TxtFileMessageProcessor : AbstractMessageProcessor
    {
        public TxtFileMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) =>
            update.Type == UpdateType.Message &&
            update.Message.Type == MessageType.Document &&
            update.Message.Document.FileName.EndsWith(".txt");

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;
            await b.SendTextMessageAsync(m.Chat.Id, "Received file with accounts! Parsing...");
            var ms = new MemoryStream();
            await b.GetInfoAndDownloadFileAsync(m.Document.FileId, ms);
            var content = Encoding.UTF8.GetString(ms.ToArray());
            //TODO:redo
            var logger = new BufferAccountsLogger();
            //var p = new FacebookTextAccountsParser(logger, content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList());
            //var accounts = p.Parse();
            //var newFlow = new BotFlow { Accounts = accounts, UserId = m.From.Id };
            await b.SendTextMessageAsync(m.Chat.Id, logger.Flush());
            await b.SendTextMessageAsync(m.Chat.Id, "Enter your proxy or proxies line by line\nFormat http(socks):192.168.0.1:6666:xxxx:yyyy");
            //return newFlow;
        }
    }
}
