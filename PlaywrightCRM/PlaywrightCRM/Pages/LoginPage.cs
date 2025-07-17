using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightCRM.Config;
using PlaywrightCRM.Core.BaseClass;
using PlaywrightCRM.Core.PlaywrightDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using static System.Net.Mime.MediaTypeNames;

namespace PlaywrightCRM.Pages
{
    internal class LoginPage : BasePageElementMap
    {
        // Đọc cấu hình từ file XML
        internal static readonly XDocument xdoc = XDocument.Load(@"Config\Config.xml");
        internal static string site = xdoc.XPathSelectElement("config/environment")?.Value ?? throw new InvalidOperationException("The 'config/environment' element is missing in the XML configuration file.");
        internal static int numberOfRuns = int.TryParse(xdoc.XPathSelectElement("config/numberOfRuns")?.Value, out int parsedValue) ? parsedValue : throw new InvalidOperationException("The 'config/numberOfRuns' element is missing or invalid in the XML configuration file.");

        // Load appsettings.json file
        internal static EnvironmentConfig GetConfigFile(string? environmentSite = null)
        {
            // Check if user provides a specific environment, if not, default at 'config.xml'
            if (environmentSite == null) environmentSite = site;

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("EnvironmentSettings")
                .Get<EnvironmentSettings>();

            if (config == null || config.Environments == null)
            {
                throw new InvalidOperationException("Configuration or Environments section is missing in appsettings.json.");
            }

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? site;

            if (!config.Environments.TryGetValue(environment, out var environmentConfig))
            {
                throw new KeyNotFoundException($"The environment '{environment}' was not found in the configuration.");
            }

            return environmentConfig;
        }

        // Variables declaration
        internal static ILocator? _txtUsername;
        internal static ILocator? _txtPassword;
        internal static ILocator? _btnLogin;
        internal static ILocator? _wrongUserNPassMsg;
        internal static ILocator? _kpiDashboardMenu;
        internal static ILocator? _menuUserName;
        internal static ILocator? _logOutDropdown;
        internal static ILocator? _logOutBtn;

        // Initiate the ILocator for elements
        public async Task HighlightElement(ILocator locator, string color = "blue", int thickness = 3)
        {
            string setOrRemoveAttr;
            if (color != "red")
            {
                setOrRemoveAttr = "removeProperty";
                await locator.EvaluateAsync($@"(e) => {{e.style.border = '{thickness}px solid {color}';}}"); Thread.Sleep(150);
                await locator.EvaluateAsync($@"(e) => {{e.style.{setOrRemoveAttr}('border');}}");
            }
            else
            {
                setOrRemoveAttr = "setProperty";
                await locator.EvaluateAsync($@"(e) => {{e.style.border = '{thickness}px solid {color}';}}"); Thread.Sleep(150);
                await locator.EvaluateAsync($@"(e) => {{e.style.{setOrRemoveAttr}('border', '{thickness}px solid {color}');}}");
            }
        }
        public ILocator TxtUsernameAsync()
        {
            if (page == null)
            {
                throw new InvalidOperationException("The page instance is null. Ensure the browser and page are properly initialized.");
            }
            return _txtUsername = page.Locator("id=login");
        }
        public ILocator TxtPassword()
        {
            if (page == null)
            {
                throw new InvalidOperationException("The page instance is null. Ensure the browser and page are properly initialized.");
            }
            return _txtPassword = page.Locator("id=password");
        }
        public ILocator BtnLogin()
        {
            if (page == null)
            {
                throw new InvalidOperationException("The page instance is null. Ensure the browser and page are properly initialized.");
            }
            return _btnLogin = page.Locator("//button[@type='submit']");
        }
        public ILocator WrongUserNPassMsg()
        {
            if (page == null)
            {
                throw new InvalidOperationException("The page instance is null. Ensure the browser and page are properly initialized.");
            }
            return _wrongUserNPassMsg = page.Locator(@"//p[@role='alert']");
        }
        public ILocator KPIDashboardMenu()
        {
            if (page == null)
            {
                throw new InvalidOperationException("The page instance is null. Ensure the browser and page are properly initialized.");
            }
            return _kpiDashboardMenu = page.Locator("//a[.='KPI Trực chat']");
        }
        public ILocator MenuUserName()
        {
            if (page == null)
            {
                throw new InvalidOperationException("The page instance is null. Ensure the browser and page are properly initialized.");
            }
            return _menuUserName = page.Locator("//div[contains(@class,'user_menu')]//span[.]");
        }
        public ILocator LogOutDropdown()
        {
            if (page == null)
            {
                throw new InvalidOperationException("The page instance is null. Ensure the browser and page are properly initialized.");
            }
            return _logOutDropdown = page.Locator("//div[contains(@class,'user_menu')]/button");
        }
        public ILocator LogOutBtn()
        {
            if (page == null)
            {
                throw new InvalidOperationException("The page instance is null. Ensure the browser and page are properly initialized.");
            }
            return _logOutBtn = page.Locator(@"//a[.='Đăng xuất']");
        }
    }

    internal sealed class LoginAction : BasePage<LoginAction, LoginPage>
    {
        #region Constructor
        private LoginAction() { }
        #endregion

        #region Items Action
        // verify elements
        public async Task<bool> IsLoginButtonShown(int timeoutInSeconds)
        {
            try
            {
                await Assertions.Expect(Map.BtnLogin()).ToBeVisibleAsync(new() { Timeout = (int)TimeSpan.FromSeconds(timeoutInSeconds).TotalMilliseconds });
                return true;
            }
            catch
            {
                await Map.HighlightElement(Map.BtnLogin(), "red");
                await Driver.TakeScreenshotAsync("ss_IsLoginBtnShown_" + DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss")); // MM-dd-yyyy_HH-mm-ss.ffftt
                return false;
            }
        }
        //public async Task<bool> UserNamePasswordMessageGetText(int timeoutInSeconds, string textParam)
        //{
        //    try
        //    {
        //        await Assertions.Expect(Map.WrongUserNPassMsg()).ToContainTextAsync(textParam, new() { Timeout = timeoutInSeconds * 1000 });
        //        return true;
        //    }
        //    catch
        //    {
        //        await Map.HighlightElement(Map.WrongUserNPassMsg(), "red");
        //        return false;
        //    }
        //}
        public async Task<bool> UserNamePasswordMessageGetText(int timeoutInSeconds, string textParam)
        {
            try
            {
                // Ensure the locator is not null before accessing it
                var wrongUserNPassMsg = Map.WrongUserNPassMsg();
                if (wrongUserNPassMsg == null)
                {
                    throw new InvalidOperationException("The 'WrongUserNPassMsg' locator is null. Ensure the page is properly initialized.");
                }

                // Await the TextContentAsync method and check if it contains the expected text
                string? textContent = await wrongUserNPassMsg.TextContentAsync(new() { Timeout = timeoutInSeconds * 1000 });
                bool element = textContent?.Contains(textParam) ?? false;

                if (element)
                {
                    await Map.HighlightElement(wrongUserNPassMsg, "green");
                }
                else
                {
                    await Map.HighlightElement(wrongUserNPassMsg, "red");
                    await Driver.TakeScreenshotAsync("ss_wrongUserNPassMsg_" + DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss"));
                }

                return element;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> IsKPIDashboardMenuShown(int timeoutInSeconds)
        {
            try
            {
                await Assertions.Expect(Map.KPIDashboardMenu()).ToBeVisibleAsync(new() { Timeout = (int)TimeSpan.FromSeconds(timeoutInSeconds).TotalMilliseconds });
                return true;
            }
            catch
            {
                await Map.HighlightElement(Map.KPIDashboardMenu(), "red");
                await Driver.TakeScreenshotAsync("ss_KPIDashboardMenu_" + DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss"));
                return false;
            }
        }
        public async Task<bool> UserNameMenuGetText(int timeoutInSeconds, string textParam)
        {
            try
            {
                await Assertions.Expect(Map.MenuUserName()).ToContainTextAsync(textParam, new() { Timeout = timeoutInSeconds * 1000 });
                return true;
            }
            catch
            {
                await Map.HighlightElement(Map.MenuUserName(), "red");
                await Driver.TakeScreenshotAsync("ss_MenuUserName_" + DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss"));
                return false;
            }
        }

        // Actions
        public async Task<LoginAction> NavigateSiteAsync(string url)
        {
            if (Map.page == null)
                throw new InvalidOperationException("Browser has not been launched.");

            await Map.page.GotoAsync(url);
            return this;
        }
        public async Task<LoginAction> InputUserName(int timeoutInSeconds, string text)
        {
            await Map.HighlightElement(Map.TxtUsernameAsync(), "blue");
            await Map.TxtUsernameAsync().FillAsync(text, new() { Timeout = timeoutInSeconds * 1000 });
            return this;
        }
        public async Task<LoginAction> InputPassword(int timeoutInSeconds, string text)
        {
            await Map.HighlightElement(Map.TxtPassword());
            await Map.TxtPassword().FillAsync(text, new() { Timeout = timeoutInSeconds * 1000 });
            return this;
        }
        public async Task<LoginAction> ClickLoginButton(int timeoutInSeconds)
        {
            await Map.HighlightElement(Map.BtnLogin());
            await Map.BtnLogin().ClickAsync(new() { Timeout = timeoutInSeconds * 1000 });
            return this;
        }
        public async Task<LoginAction> ClickLogOutDropdown(int timeoutInSeconds)
        {
            await Map.HighlightElement(Map.LogOutDropdown());
            await Map.LogOutDropdown().ClickAsync(new() { Timeout = timeoutInSeconds * 1000 });
            return this;
        }
        public async Task<LoginAction> ClickLogOutBtn(int timeoutInSeconds)
        {
            await Map.HighlightElement(Map.LogOutBtn());
            await Map.LogOutBtn().ClickAsync(new() { Timeout = timeoutInSeconds * 1000 });
            return this;
        }
        #endregion

        #region Built-in Actions
        public async Task<LoginAction> LoginSite(int timeoutInSeconds, string? url = null, string? username = null, string? password = null)
        {
            // Check if user login with a specific URL, if not, get URL from appsettings.json (default)
            url ??= LoginPage.GetConfigFile().BaseUrl;
            username ??= LoginPage.GetConfigFile().username;
            password ??= LoginPage.GetConfigFile().password;

            await NavigateSiteAsync(url);
            await InputUserName(timeoutInSeconds, username);
            await InputPassword(timeoutInSeconds, password);
            await ClickLoginButton(timeoutInSeconds);
            return this;
        }

        public async Task<LoginAction> ClicklogOut(int timeoutInSeconds)
        {
            await ClickLogOutDropdown(timeoutInSeconds);
            await ClickLogOutBtn(timeoutInSeconds);
            return this;
        }
        #endregion
    }
}
