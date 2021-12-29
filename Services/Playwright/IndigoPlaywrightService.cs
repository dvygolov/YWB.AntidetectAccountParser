using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using YWB.AntidetectAccountParser.Model;

namespace YWB.AntidetectAccountParser.Services.Playwright
{
    public class IndigoPlaywrightService
    {
        //private async Task<(string token, string accId)> GetTokenAndAccountIdAsync(RemoteWebDriver driver, FacebookAccount fa)
        //{
        //    driver.Navigate().GoToUrl("https://facebook.com/adsmanager");
        //    while (driver.Url.Contains("content_management"))
        //        driver.Navigate().GoToUrl("https://facebook.com/adsmanager");
        //    if (driver.Url.StartsWith("https://www.facebook.com/index.php")) //мы на странице логина
        //    {
        //        driver.Navigate().GoToUrl("https://mobile.facebook.com");
        //        var origTimeout = driver.Manage().Timeouts().ImplicitWait;
        //        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
        //        var loginBtn = driver.FindElementsByClassName("_mDeviceLoginHomepage__userNameAndBadge");
        //        if (loginBtn.Count > 0)
        //        {
        //            loginBtn[0].Click();
        //            var input = driver.FindElements(By.XPath("//input[@type='password']"));
        //            input[0].SendKeys(fa.Password);
        //            input[0].SendKeys(Keys.Enter);
        //            await Task.Delay(5000);
        //        }
        //        var cookieButton = driver.FindElements(By.XPath("//a[@data-cookiebanner='accept_button']"));
        //        if (cookieButton.Count > 0)
        //            cookieButton[0].Click();
        //        var loginInput = driver.FindElementsById("m_login_email");
        //        var passInput = driver.FindElementsById("m_login_password");
        //        if (loginInput.Count > 0 && passInput.Count > 0)
        //        {
        //            loginInput[0].SendKeys(fa.Login);
        //            passInput[0].SendKeys(fa.Password);
        //            var l2btn = driver.FindElements(By.XPath("//button[@name='login']"));
        //            l2btn[0].Click();
        //        }
        //        driver.Navigate().GoToUrl("https://facebook.com/adsmanager");
        //        while (driver.Url.Contains("content_management"))
        //            driver.Navigate().GoToUrl("https://facebook.com/adsmanager");
        //    }
        //    var getTokenScript = @"
        //        var re = new RegExp('EAABsbCS1[a-zA-Z0-9]*');
        //        var m = document.documentElement.innerHTML.match(re);
        //        return m[0];";
        //    var token = (string)driver.ExecuteScript(getTokenScript);
        //    var accId = HttpUtility.ParseQueryString(new Uri(driver.Url).Query).Get("act");
        //    return (token, accId);
        //}

        //public async Task<string> GetTokenAsync(string profileId,FacebookAccount fa, bool isFirefox = false)
        //{
        //    var driver = await ConnectWebDriver(profileId, isFirefox, true);
        //    if (driver == null) return null;

        //    (var token, _) = await GetTokenAndAccountIdAsync(driver,fa);
        //    await StopAsync(profileId, driver);
        //    if (string.IsNullOrEmpty(token))
        //        Console.WriteLine("Не удалось получить токен!!!");
        //    else
        //        Console.WriteLine("Токен фб получен!");

        //    return token;
        //}

        //public async Task<bool> LoginToFacebookAsync(string profileId, string login, string password, bool isFirefox = false)
        //{
        //    var driver = await ConnectWebDriver(profileId, isFirefox, true);
        //    if (driver == null) return false;
        //    driver.Navigate().GoToUrl("https://facebook.com");
        //    return true;
        //}

        //public async Task<string> GetFacebookCookiesAsync(string profileId, bool isFirefox = false)
        //{
        //    var driver = await ConnectWebDriver(profileId, isFirefox, true);
        //    if (driver == null) return null;
        //    driver.Navigate().GoToUrl("https://facebook.com");
        //    var cookies = driver.Manage().Cookies.AllCookies;
        //    var cookiesJson = "[";
        //    for (var i = 0; i < cookies.Count; i++)
        //    {
        //        var c = cookies[i];
        //        cookiesJson += CookieHelper.SeleniumCookiesToJSON(c);
        //        if (i != cookies.Count - 1)
        //            cookiesJson += ",";
        //    }
        //    cookiesJson += "]";

        //    await StopAsync(profileId, driver);
        //    return cookiesJson;
        //}

        /*private async Task<bool> ProfileIsActiveAsync(string profileId)
        {
            try
            {
                var r = new RestRequest("active", Method.GET);
                r.AddQueryParameter("profileId", profileId);
                var res = await _rc.ExecuteAsync(r);
                var json = JsonConvert.DeserializeObject<JObject>(res.Content);
                return json["value"].ToString().ToLowerInvariant() == "true";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task StopAsync(string profileId, RemoteWebDriver driver = null)
        {
            var r = new RestRequest("stop", Method.GET);
            r.AddQueryParameter("profileId", profileId);
            await _rc.ExecuteAsync(r);
            driver?.Quit();
        }

        private async Task<string> StartAndGetAddressAsync(string profileId)
        {
            var r = new RestRequest("start", Method.GET);
            r.AddQueryParameter("profileId", profileId);
            r.AddQueryParameter("loadTabs", "true");
            r.AddQueryParameter("automation", "true");
            r.AddQueryParameter("puppeteer", "true");
            dynamic json = await ExecuteRequestAsync<JObject>(r);
            return json.value;
        }

        private async Task<RemoteWebDriver> ConnectWebDriver(string profileId, bool isFirefox = false, bool headless = false)
        {
            if (await ProfileIsActiveAsync(profileId))
                await StopAsync(profileId);

            var address = await StartAndGetAddressAsync(profileId);
            if (string.IsNullOrEmpty(address)) return null;
            using var playwright = await Playwright.CreateAsync();
            var chromium = playwright.Chromium;
            await chromium.LaunchAsync(new BrowserTypeLaunchOptions { });
                var op = new ChromeOptions();

                if (headless)
                    op.AddArgument("headless");
                driver = new RemoteWebDriver(new Uri(address), op.ToCapabilities(), TimeSpan.FromMinutes(5));

            return driver;
        }

        private async Task<T> ExecuteRequestAsync<T>(RestRequest r, string url = "http://localhost:35000/api/v1/profile")
        {
            var rc = new RestClient(url);
            IRestResponse resp;
            int tryCount = 0;
            do
            {
                resp = await rc.ExecuteAsync(r, new CancellationToken());
                tryCount++;
            }
            while (tryCount <= 3 && resp.StatusCode != System.Net.HttpStatusCode.OK);

            if (resp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Can't start execute request.");
                Console.WriteLine(resp.Content);
                return default(T);
            }

            T res = default(T);
            try
            {
                res = JsonConvert.DeserializeObject<T>(resp.Content);
            }
            catch (Exception)
            {
                Console.WriteLine($"Error deserializing {resp.Content} to {typeof(T)}");
                throw;
            }
            return res;
        }

        internal Task GetTokensAsync(Dictionary<string, FacebookAccount> profiles)
        {

        }*/
    }
}
