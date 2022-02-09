using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class FilledFlowMessageProcessor : AbstractMessageProcessor
    {
        public FilledFlowMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) => flow.IsFilled();

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;
            await b.SendTextMessageAsync(m.Chat.Id, "All data filled, starting import, PLEASE WAIT!");
            await flow.Importer.ImportAccountsAsync(flow.Accounts, flow);
            await b.SendTextMessageAsync(
                chatId: m.Chat.Id,
                text: "All done, HAPPY HACKING!",
                replyMarkup: new ReplyKeyboardRemove());
        }
    }
}
