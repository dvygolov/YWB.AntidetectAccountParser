using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class UnknownFileMessageProcessor : AbstractMessageProcessor
    {
        public UnknownFileMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) =>
            update.Type == UpdateType.Message &&
            update.Message.Type == MessageType.Document &&
            !update.Message.Document.FileName.EndsWith(".txt") &&
            !update.Message.Document.FileName.EndsWith(".xlsx");

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;
            await b.SendTextMessageAsync(m.Chat.Id, "Unknown file type!");
        }
    }
}
