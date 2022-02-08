using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Services.Parsers;
using YWB.AntidetectAccountsParser.Services.Proxies;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class ProxyMessageProcessor : AbstractMessageProcessor
    {
        public ProxyMessageProcessor(IServiceProvider sp) : base(sp) { }

        public override bool Filter(BotFlow flow, Update update) => flow.Proxies == null;

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;

            Message sentMessage = await b.SendTextMessageAsync(
                chatId: m.Chat.Id,
                text: "Checking accounts, please wait!",
                cancellationToken: ct);

            var pp = new TextProxyProvider(m.Text);
            flow.Proxies = pp.Get();
            var ap = new FacebookTextAccountsParser(pp, _sp.GetService<ILogger>(), flow.AccountStrings);
            flow.Accounts = ap.Parse();
            var services = _sp.GetService<List<ServiceCredentials>>();
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                services.Select(s=>InlineKeyboardButton.WithCallbackData(s.Name))
            });
            sentMessage = await b.SendTextMessageAsync(
                chatId: m.Chat.Id,
                text: "Choose, where to import your accounts:",
                replyMarkup: inlineKeyboard,
                cancellationToken: ct);
        }
    }
}
