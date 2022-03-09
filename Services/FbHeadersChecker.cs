using System;
using System.Net.Http;

namespace YWB.AntidetectAccountParser.Services
{
    internal class FbHeadersChecker
    {
        public static bool Check(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("c_user cookie is empty!");
                return true;
            }
            var url = $"https://mbasic.facebook.com/profile.php?id={id}";
            try
            {
                var hc = new HttpClient();
                hc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 14_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) GSA/190.0.415307624 Mobile/15E148 Safari/604.1");
                var resp = hc.Send(new HttpRequestMessage(HttpMethod.Head, url));
                switch (resp.StatusCode)
                {
                    case System.Net.HttpStatusCode.Redirect:
                        if (resp.Headers.Contains("X-XSS-Protection"))
                        {
                            Console.WriteLine($"Account {id} is blocked in Facebook!");
                            return false;
                        }
                        Console.WriteLine($"Account {id} is OK!");
                        return true;
                    case System.Net.HttpStatusCode.OK:
                        if (resp.Headers.Contains("X-XSS-Protection"))
                        {
                            Console.WriteLine($"Account {id} is OK!");
                            return true;
                        }
                        Console.WriteLine($"Account {id} doesn't exist in Facebook!");
                        return false;
                    default:
                        Console.WriteLine($"Error getting account's {id} status from Facebook!");
                        return true;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error getting account's {id} status from Facebook: {e}");
                return true;
            }
        }
    }
}
