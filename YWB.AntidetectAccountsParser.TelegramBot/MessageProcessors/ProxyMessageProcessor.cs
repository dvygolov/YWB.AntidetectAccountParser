using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YWB.AntidetectAccountsParser.Interfaces;
using YWB.AntidetectAccountsParser.Model;
using YWB.AntidetectAccountsParser.Model.Accounts;
using YWB.AntidetectAccountsParser.Services.Parsers;
using YWB.AntidetectAccountsParser.Services.Proxies;

namespace YWB.AntidetectAccountsParser.TelegramBot.MessageProcessors
{
    public class ProxyMessageProcessor : AbstractMessageProcessor
    {
        private readonly List<ServiceCredentials> _credentials;
        private readonly IProxyProvider<SocialAccount> _pp;

        public ProxyMessageProcessor(List<ServiceCredentials> credentials, IProxyProvider<SocialAccount> proxyProvider)
        {
            _credentials = credentials;
            _proxyProvider = proxyProvider;
        }

        public override bool Filter(BotFlow flow, Update update) => flow.AccountsDataProvider!=null && flow.Proxies == null;

        public override async Task PayloadAsync(BotFlow flow, Update update, ITelegramBotClient b, CancellationToken ct)
        {
            var m = update.Message;

            _pp.SetSource(m.Text);
            try
            {
                flow.Proxies = _pp.Get();
            }
            catch
            {
                await b.SendTextMessageAsync(
                    chatId: m.Chat.Id,
                    text: "Invalid proxy format! Try again.",
                    cancellationToken: ct);
                return;
            }

            Message sentMessage = await b.SendTextMessageAsync(
                chatId: m.Chat.Id,
                text: "Checking accounts, please wait!",
                cancellationToken: ct);
            var ap = new FacebookTextAccountsParser(_pp, flow.AccountsDataProvider,_sp.GetService<ILogger>());
            flow.Accounts = ap.Parse();
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                _credentials.Select(s=>InlineKeyboardButton.WithCallbackData(s.Name))
            });
            sentMessage = await b.SendTextMessageAsync(
                chatId: m.Chat.Id,
                text: "Choose, where to import your accounts:",
                replyMarkup: inlineKeyboard,
                cancellationToken: ct);
        }
    }
}
