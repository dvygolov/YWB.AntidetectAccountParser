using Topshelf;
using YWB.AntidetectAccountsParser.TelegramBot;

var rc = HostFactory.Run(x =>
{
    x.Service<BotServiceStarter>(s =>
    {
        s.ConstructUsing(_ => new BotServiceStarter());
        s.WhenStarted(host => host.OnStart());
        s.WhenStopped(host => host.OnStop());
    });
    x.RunAsLocalSystem();
    x.StartAutomatically();
    x.EnableServiceRecovery(src => src.RestartService(5));

    x.SetDescription("Antidetect Accounts Parser Telegram Bot by Yellow Web");
    x.SetDisplayName("YWB.AAP.Telegram.Bot");
    x.SetServiceName("YWB.AAP.Telegram.Bot");
});
var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
Environment.ExitCode = exitCode;
