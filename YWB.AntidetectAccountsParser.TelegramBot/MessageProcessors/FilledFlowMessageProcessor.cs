using Telegram.Bot;
using Telegram.Bot.Types;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class FilledFlowMessageProcessor : AbstractMessageProcessor
    {
        public FilledFlowMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) => flow?.IsFilled() ?? false;

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;
            await flow.Importer.ImportAccountsAsync(flow.Accounts, flow);
            await b.SendTextMessageAsync(m.Chat.Id, "All done, HAPPY HACKING!");
        }
    }
}
