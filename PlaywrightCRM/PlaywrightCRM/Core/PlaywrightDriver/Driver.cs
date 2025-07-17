using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Playwright;
using PlaywrightCRM.Config;
using RazorEngine.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PlaywrightCRM.Core.PlaywrightDriver
{
    public class Driver : IDisposable
    {
        internal static readonly XDocument xdoc = XDocument.Load(@"Config\Config.xml");
        protected EnvironmentConfig? EnvConfig;
        internal static string folderName = xdoc.XPathSelectElement("config/recordvideo/recordvideoPath").Value;
        internal static string recordVideoPath = Path.GetFullPath(@"../../../../" + folderName + "/");
        internal static string videoOutputPath = recordVideoPath + "RecordVideo";
        private static IPlaywright? _playwright;
        private static IBrowser? _browser;
        internal static IBrowserContext? _browserContext;
        public static IPage? _page;

        public static IBrowser? Browser
        {
            get
            {
                if (_browser == null)
                { throw new NullReferenceException("The IBrowser browser instance was not initialized. You should first call the method class"); }
                return _browser;
            }
            private set { _browser = value; }
        }

        // Khởi chạy trình duyệt
        public async Task StartBrowser(string? browserType = null, bool? headless = null, float? slowMo = null)
        {
            // Đọc cấu hình từ file XML
            var environmentElement = xdoc.XPathSelectElement("config/environment");

            if (environmentElement == null)
            {
                throw new InvalidOperationException("The 'config/environment' element is missing in the XML configuration file.");
            }

            string site = environmentElement.Value;

            // Đọc cấu hình
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("EnvironmentSettings")
                .Get<EnvironmentSettings>();

            // Lấy môi trường hiện tại (có thể dùng biến môi trường)
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? site;
            EnvConfig = config.Environments[environment];

            var browser = EnvConfig.Browser;

            // Using chromium browser by default, if no then get default browser at 'appsetting.json'
            if (browserType == null) browserType = browser;

            _playwright = await Playwright.CreateAsync();

            // Chọn trình duyệt
            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = EnvConfig.Headless,
                SlowMo = EnvConfig.Timeout
            };

            Browser = browserType.ToLower() switch
            {
                "chromium" => await _playwright.Chromium.LaunchAsync(launchOptions),
                "firefox" => await _playwright.Firefox.LaunchAsync(launchOptions),
                "webkit" => await _playwright.Webkit.LaunchAsync(launchOptions),
                _ => throw new ArgumentException("Trình duyệt không hợp lệ / Invalid browser type.")
            };

            // Tạo một trang mới trong trình duyệt
            _page = await Browser.NewPageAsync();
        }

        // Khởi chạy trình duyệt với record video
        public async Task StartBrowserWithRecordVideo(string? browserType = null, bool? headless = null, float? slowMo = null)
        {
            // Đọc cấu hình từ file XML
            var environmentElement = xdoc.XPathSelectElement("config/environment") ?? throw new InvalidOperationException("The 'config/environment' element is missing in the XML configuration file.");
            string site = environmentElement.Value;

            // Đọc cấu hình
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("EnvironmentSettings")
                .Get<EnvironmentSettings>();

            // Lấy môi trường hiện tại (có thể dùng biến môi trường)
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? site;
            EnvConfig = config.Environments[environment];

            var browser = EnvConfig.Browser;

            // Using chromium browser by default, if no then get default browser at 'appsetting.json'
            if (browserType == null) browserType = browser;

            _playwright = await Playwright.CreateAsync();

            // Chọn trình duyệt
            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = EnvConfig.Headless,
                SlowMo = EnvConfig.Timeout
            };

            Browser = browserType.ToLower() switch
            {
                "chromium" => await _playwright.Chromium.LaunchAsync(launchOptions),
                "firefox" => await _playwright.Firefox.LaunchAsync(launchOptions),
                "webkit" => await _playwright.Webkit.LaunchAsync(launchOptions),
                _ => throw new ArgumentException("Trình duyệt không hợp lệ / Invalid browser type.")
            };

            // Record video
            // Tạo thư mục lưu video nếu chưa tồn tại
            Directory.CreateDirectory(videoOutputPath);

            _browserContext = await (Browser.NewContextAsync(new BrowserNewContextOptions
            {
                RecordVideoDir = videoOutputPath, // Directory to save the video
                RecordVideoSize = new RecordVideoSize { Width = 1280, Height = 720 }, // Video size
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 } // Viewport size
            }) ?? throw new NullReferenceException("Browser instance is null."));

            // Tạo một trang mới trong trình duyệt
            _page = await _browserContext.NewPageAsync();
        }

        // Property để truy xuất trang hiện tại
        public static IPage? Page => _page;
        public static IBrowserContext? BrowserContext => _browserContext;

        // Giải phóng tài nguyên
        public void Dispose()
        {
            Page?.CloseAsync().GetAwaiter().GetResult(); // _page
            Browser?.CloseAsync().GetAwaiter().GetResult(); // _browser
            BrowserContext?.CloseAsync();
            _playwright?.Dispose();
        }

        public static async Task TakeScreenshotAsync(string screenShotName) // ILocator locator
        {
            string folderName = xdoc.XPathSelectElement("config/screenshot/screenshotPath").Value;
            string screenshotPath = Path.GetFullPath(@"../../../../" + folderName + "/"); ;
            string screenshotFormat = xdoc.XPathSelectElement("config/screenshot/screenshotFormat").Value;
            string filePath = screenshotPath + screenShotName + screenshotFormat;

            // Kiểm tra xem file path có hợp lệ không
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            // Chụp ảnh màn hình và lưu vào đường dẫn chỉ định
            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = filePath,
                FullPage = true // Chụp toàn bộ trang, nếu muốn
            });
        }

        public static async Task TakeScreenshotILocatorAsync(ILocator locator, string screenShotName)
        {
            string folderName = xdoc.XPathSelectElement("config/screenshot/screenshotPath").Value;
            string screenshotPath = Path.GetFullPath(@"../../../../" + folderName + "/");
            string screenshotFormat = xdoc.XPathSelectElement("config/screenshot/screenshotFormat").Value;
            string filePath = screenshotPath + screenShotName + screenshotFormat;

            // If options are not provided, use default LocatorScreenshotOptions
            LocatorScreenshotOptions options = new()
            {
                Path = filePath // Specify the file path for the screenshot
            };

            // Capture screenshot from the locator
            await locator.ScreenshotAsync(options);
        }
    }
}
