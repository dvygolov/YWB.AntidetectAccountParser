using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Services.Browsers;
using YWB.AntidetectAccountsParser.Services.Monitoring;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class ImporterMessageProcessor : AbstractMessageProcessor
    {
        public ImporterMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) =>
            update.Type == UpdateType.CallbackQuery &&
            flow.Importer == null;

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var credentials = _sp.GetService<List<ServiceCredentials>>();
            var fromId = update.CallbackQuery.From.Id;
            if (update.CallbackQuery.Data.Contains("Exit"))
            {
                flow.Clear();
                await b.SendTextMessageAsync(
                    chatId: fromId,
                    text: "All done, HAPPY HACKING!",
                    replyMarkup: new ReplyKeyboardRemove());
                return;
            }
            var current = credentials.First(c => c.Name == update.CallbackQuery.Data).Credentials;
            flow.Importer = update.CallbackQuery.Data switch
            {
                "Indigo" => new IndigoApiService(current),
                "AdsPower" => new AdsPowerApiService(current),
                "DolphinAnty" => new DolphinAntyApiService(current),
                "Octo" => new OctoApiService(current),
                string fbTool when fbTool.StartsWith("FbTool") => new FbToolService(current),
                "Dolphin" => new DolphinService(current),
                _ => throw new Exception("Invalid importing service name!")
            };

            if (flow.IsFilled())
            {
                await b.SendTextMessageAsync(fromId, "All data filled, starting import, PLEASE WAIT!");
                await flow.Importer.ImportAccountsAsync(flow.Accounts, flow);
                await b.SendTextMessageAsync(
                    chatId: fromId,
                    text: "All done, HAPPY HACKING!",
                    replyMarkup: new ReplyKeyboardRemove());
                return;
            }

            var oses = flow.Importer.GetOsList();
            if (oses != null)
            {
                InlineKeyboardMarkup inlineKeyboard = new(new[]
                {
                    oses.Select(os=>InlineKeyboardButton.WithCallbackData(os))
                });
                await b.SendTextMessageAsync(
                    chatId: fromId,
                    text: "Choose your OS:",
                    replyMarkup: inlineKeyboard);
            }
            else
            {
                flow.Os = "None";
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
}
