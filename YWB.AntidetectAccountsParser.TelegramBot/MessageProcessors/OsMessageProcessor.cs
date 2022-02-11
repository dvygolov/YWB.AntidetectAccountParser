using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class OsMessageProcessor : AbstractMessageProcessor
    {
        public override bool Filter(BotFlow flow, Update update) =>
            update.Type == UpdateType.CallbackQuery &&
            flow.Importer != null &&
            string.IsNullOrEmpty(flow.Os);

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var fromId = update.CallbackQuery.From.Id;
            var oses = flow.Importer.GetOsList();
            if (!oses.Contains(update.CallbackQuery.Data))
            {
                await b.SendTextMessageAsync(
                    chatId: fromId,
                    text: "Invalid OS! Choose again!");
                return;
            }
            flow.Os = update.CallbackQuery.Data;
            var groups = await flow.Importer.GetExistingGroupsAsync();
            var buttons = groups.Where(g => g.Name != null).OrderBy(g => g.Name)
                .Select(g => InlineKeyboardButton.WithCallbackData(g.Name, g.Name)).Chunk(6).ToArray();
            InlineKeyboardMarkup inlineKeyboard = new(buttons);
            await b.SendTextMessageAsync(
                chatId: fromId,
                text: "Choose a group, where the accounts will be imported:",
                replyMarkup: inlineKeyboard);
        }
    }
}
