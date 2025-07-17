using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using PlaywrightCRM.Core.PlaywrightDriver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PlaywrightCRM.Core.BaseTestCase
{
    [TestFixture]
    internal class BaseTestCase
    {
        private TestContext? context;
        public static bool verifyPoint;
        public static Dictionary<string, bool> verifyPoints = new Dictionary<string, bool>();
        public static Driver driver = new();
        internal static readonly XDocument xdoc = XDocument.Load(@"Config\Config.xml");

        [Obsolete]
        internal static ExtentSparkReporter? htmlReporter;
        public static ExtentReports? extent;
        public static ExtentTest? test;

        public TestContext TestContext
        {
            get { return context; }
            set { context = value; }
        }

        [SetUp]
        public virtual async Task SetupTestAsync()
        {
            verifyPoints.Clear();
            await driver.StartBrowserWithRecordVideo(headless: false);
        }

        [Obsolete]
        public static ExtentReports ExtReportgetInstance(string? fileName = null)
        {
            // Default fileName is 'Results'
            if (fileName == null) fileName = "Results";

            if (extent == null)
            {
                string? instanceName = "qa-odoo"; //;LoginPage.instanceName;
                string fileExportPath = Path.GetFullPath(@"../../../../TestResults/");                
                string reportFile = $"{DateTime.Now:yyyyMMdd_HHmmss}" + @"_" + fileName + ".html";
                htmlReporter = new ExtentSparkReporter(fileExportPath + reportFile); // ExtentV3HtmlReporter
                extent = new ExtentReports();
                extent.AttachReporter(htmlReporter);
                extent.AddSystemInfo("OS", "Windows");
                extent.AddSystemInfo("Host Name", "GenD-Connext");
                extent.AddSystemInfo("Environment", instanceName);

                // Load Extent Report Config xml file
                string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Reports\");
                string extentConfigPath = filePath + "ExtentReportConfig.xml";
                htmlReporter.LoadConfig(extentConfigPath);
            }
            return extent;
        }

        public static void ExtReportResult(bool verifyPoint, string descriptionTC)
        {
            if (verifyPoint == false)
            {
                test.Log(Status.Fail, descriptionTC);
            }
            if (verifyPoint == true)
            {
                test.Log(Status.Pass, descriptionTC);
            }
        }

        [TearDown]
        public virtual void TeardownTest()
        {
            driver.Dispose();
            EndReport();

            #region Result (Passed or Failed)
            bool result = true;
            foreach (var verify in verifyPoints)
            {
                string step_result = verify.Value ? "Pass" : "Fail";
                Console.WriteLine(verify.Key + " : " + step_result);
                result = result && verify.Value;
            }
            Assert.That(result);
            #endregion
        }

        public void EndReport()
        {
            // Write content to report file
            extent.Flush();

            // Get file (Result) path
            string fileExportPath = Path.GetFullPath(@"../../../../TestResults/");

            // Rename a file (default name of file result is 'index.html') --> This only applies to ExtentReport V4
            if (File.Exists(fileExportPath + "index.html"))
            {
                string fileName = "Results";
                string reportFile = DateTime.Now.ToString().Replace("/", "_").Replace(":", "_").Replace(" ", "_") + @"_" + fileName + ".html";
                File.Move(fileExportPath + "index.html", fileExportPath + reportFile);
            }

            // Open Report html file after executing all TCs
            var file = new DirectoryInfo(fileExportPath).GetFiles("*.html").OrderByDescending(f => f.LastWriteTime).First();
            var chrome64 = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            var chrome32 = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            if (File.Exists(chrome64))
            {
                //Solution 1:
                //System.Diagnostics.Process.Start(chrome64, " --start-maximized " + @"" + file.ToString() + "" + " --incognito");

                //Solution 2:
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Minimized;
                startInfo.FileName = chrome64;
                startInfo.Arguments = " --start-maximized " + @"" + file.ToString() + "" + " --incognito";
                process.StartInfo = startInfo;
                process.Start();
                Thread.Sleep(1000);
                IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, "AutomationTesting.in - Google Chrome");
                ShowWindow(hwnd, 6); // minimize = 6; maximize =3
            }
            if (File.Exists(chrome32))
            {
                //Solution 1:
                //System.Diagnostics.Process.Start(chrome32, " --start-maximized " + @"" + file.ToString() + "" + " --incognito");

                //Solution 2:
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = chrome32;
                startInfo.Arguments = " --start-maximized " + @"" + file.ToString() + "" + " --incognito";
                process.StartInfo = startInfo;
                process.Start();
                Thread.Sleep(1000);
                IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, "AutomationTesting.in - Google Chrome");
                ShowWindow(hwnd, 6); // minimize = 6; maximize =3
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        public static async Task DeleteFolderRecordVideoAsync()
        {
            driver.Dispose();

            string folderPath = xdoc.XPathSelectElement("config/recordvideo/recordvideoPath").Value;
            string parentPath = Path.GetFullPath(@"../../../../" + folderPath + "/");
            string folderVideo = "RecordVideo";

            #region Đổi Tên file video
            // Ensure Page and Video are not null before accessing them
            if (Driver.Page?.Video == null)
            {
                throw new NullReferenceException("Page or Video is null. Ensure the browser and video recording are properly initialized.");
            }

            // Get the video path (Lấy đường dẫn file video)
            var videoPath = await Driver.Page.Video.PathAsync();

            // Tạo tên mới cho file video (ví dụ: dựa trên thời gian hoặc tên test)
            string newFileName = $"TestVideo_{DateTime.Now:yyyyMMdd_HHmmss}.webm";
            //string newFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_RecordVideo.webm";
            string newFilePath = Path.Combine(Driver.videoOutputPath, newFileName); // example: videos/TestVideo_20231001_123456.webm

            // Đổi tên file
            File.Move(videoPath, newFilePath);
            #endregion

            #region Xóa thư mục video đã ghi lại
            // Use Directory.GetDirectories to find matching folders
            string[] matchingFolders = Directory.GetDirectories(parentPath, $"*{folderVideo}*", SearchOption.TopDirectoryOnly);

            foreach (string folder in matchingFolders)
            {
                if (Directory.Exists(folder))
                {
                    // Xóa tất cả file trong thư mục
                    foreach (string file in Directory.GetFiles(folder))
                    {
                        if (file.Contains(newFileName))
                        {
                            File.Delete(file);
                        }
                    }

                    // Xóa thư mục
                    //Directory.Delete(folder, true);
                }
            }
            #endregion
        }
    }
}
