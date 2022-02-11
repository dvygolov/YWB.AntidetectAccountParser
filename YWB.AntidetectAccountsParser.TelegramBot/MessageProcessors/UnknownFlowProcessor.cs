using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class UnknownFlowProcessor : AbstractMessageProcessor
    {
        public override bool Filter(BotFlow flow, Update update) => flow.IsEmpty();

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var fromId = update.Message?.From.Id??update.CallbackQuery?.From.Id;
            await b.SendTextMessageAsync(
                chatId: fromId.Value,
                text: "Unknown command! Send your accounts file first!",
                replyMarkup: new ReplyKeyboardRemove());
        }
    }
}
