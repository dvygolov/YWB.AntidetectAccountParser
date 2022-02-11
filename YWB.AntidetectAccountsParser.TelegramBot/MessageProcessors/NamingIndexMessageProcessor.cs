using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Services.Browsers;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class NamingIndexMessageProcessor : AbstractMessageProcessor
    {
        private readonly List<ServiceCredentials> _credentials;

        public NamingIndexMessageProcessor(List<ServiceCredentials> credentials)
        {
            _credentials = credentials;
        }

        public override bool Filter(BotFlow flow, Update update) =>
            flow.Group != null &&
            !string.IsNullOrEmpty(flow.NamingPrefix) &&
            flow.NamingIndex == null;

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;
            flow.NamingIndex = int.Parse(m.Text);
            await b.SendTextMessageAsync(m.Chat.Id, "All data filled, starting import, PLEASE WAIT!");
            if (flow.IsFilled())
            {
                await flow.Importer.ImportAccountsAsync(flow.Accounts, flow);
                var credentials = _credentials.Where(c => c.Name.StartsWith("FbTool") || c.Name == "Dolphin");
                if (flow.Importer is AbstractAntidetectApiService &&
                    credentials.Any() &&
                    flow.Accounts.All(a => !string.IsNullOrEmpty((a as FacebookAccount).Token)))
                {
                    flow.Importer = null;
                    flow.Group = null;
                    InlineKeyboardMarkup inlineKeyboard = new(new[]
                    {
                        credentials.Select(s=>InlineKeyboardButton.WithCallbackData(s.Name))
                        .Append(InlineKeyboardButton.WithCallbackData("Exit"))
                    });
                    await b.SendTextMessageAsync(
                        chatId: m.Chat.Id,
                        text: "All of your accounts have tokens! Do you want to add them to a monitoring service?",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: ct);
                }
                else
                {
                    flow.Clear();
                    await b.SendTextMessageAsync(
                        chatId: m.Chat.Id,
                        text: "All done, HAPPY HACKING!",
                        replyMarkup: new ReplyKeyboardRemove());
                }
            }
            else
                await b.SendTextMessageAsync(m.Chat.Id, "Flow not filled!");
        }
    }
}
