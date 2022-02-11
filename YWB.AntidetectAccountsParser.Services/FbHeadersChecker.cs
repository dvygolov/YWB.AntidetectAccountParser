using Microsoft.Extensions.Logging;

namespace YWB.AntidetectAccountsParser.Services
{
    public class FbHeadersChecker
    {
        private readonly ILogger<FbHeadersChecker> _logger;

        public FbHeadersChecker(ILogger<FbHeadersChecker> logger)
        {
            _logger = logger;
        }

        public bool Check(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogInformation("c_user cookie is empty!");
                return true;
            }
            var url = $"https://mbasic.facebook.com/profile.php?id={id}";
            var hc = new HttpClient();
            hc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 14_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) GSA/190.0.415307624 Mobile/15E148 Safari/604.1");
            var resp = hc.Send(new HttpRequestMessage(HttpMethod.Head, url));
            switch (resp.StatusCode)
            {
                case System.Net.HttpStatusCode.Redirect:
                    if (resp.Headers.Contains("X-XSS-Protection"))
                    {
                        _logger.LogInformation($"Account {id} is blocked in Facebook!");
                        return false;
                    }
                    _logger.LogInformation($"Account {id} is OK!");
                    return true;
                case System.Net.HttpStatusCode.OK:
                    if (resp.Headers.Contains("X-XSS-Protection"))
                    {
                        _logger.LogInformation($"Account {id} is OK!");
                        return true;
                    }
                    _logger.LogInformation($"Account {id} doesn't exist in Facebook!");
                    return false;
                default:
                    _logger.LogInformation($"Error getting account's {id} status from Facebook!");
                    return true;
            }
        }
    }
}
