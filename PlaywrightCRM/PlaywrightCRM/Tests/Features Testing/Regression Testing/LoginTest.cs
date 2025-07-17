using AventStack.ExtentReports;
using PlaywrightCRM.Core.BaseTestCase;
using PlaywrightCRM.Core.PlaywrightDriver;
using PlaywrightCRM.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightCRM.Tests.Features_Testing.Regression_Testing
{
    [TestFixture, Order(1)]
    internal class LoginTest : BaseTestCase
    {
        [Obsolete]
        private readonly ExtentReports rep = ExtReportgetInstance();
        private string? summaryTC;

        [SetUp]
        public override async Task SetupTestAsync()
        {
            verifyPoints.Clear();
            //await driver.StartBrowser();
            await driver.StartBrowserWithRecordVideo();
        }

        [Test, Category("Regression Tests")]
        public async Task TC001_login_with_invalid_valid_accountAsync()
        {
            #region Declare variables
            string url = LoginPage.GetConfigFile().BaseUrl;
            string username = LoginPage.GetConfigFile().username;
            string password = LoginPage.GetConfigFile().password;
            string fullname = LoginPage.GetConfigFile().fullname;
            #endregion

            #region Workflow scenario
            test = rep.CreateTest("Connext - Login Test 001");
            try
            {
                await LoginAction.Instance.NavigateSiteAsync(url);
                verifyPoint = await LoginAction.Instance.IsLoginButtonShown(5);
                verifyPoints.Add(summaryTC = "Verify Login button (Dang Nhap) is displayed after navigating to Website", verifyPoint);
                ExtReportResult(verifyPoint, summaryTC);

                #region login test - invalid account
                await LoginAction.Instance.InputUserName(10, "abc");
                await LoginAction.Instance.InputPassword(10, "acb@email.com");
                await LoginAction.Instance.ClickLoginButton(10);

                string msg = "Wrong login/password";
                verifyPoint = await LoginAction.Instance.UserNamePasswordMessageGetText(5, msg);
                verifyPoints.Add(summaryTC = "Verify the message '" + msg + "' is shown", verifyPoint);
                ExtReportResult(verifyPoint, summaryTC);
                #endregion

                #region Login test - valid account
                await LoginAction.Instance.InputUserName(10, username);
                await LoginAction.Instance.InputPassword(10, password);
                await LoginAction.Instance.ClickLoginButton(10);

                verifyPoint = await LoginAction.Instance.IsKPIDashboardMenuShown(10);
                verifyPoints.Add(summaryTC = "Verify 'KPI Dashboard' menu is shown after login successfully", verifyPoint);
                ExtReportResult(verifyPoint, summaryTC);

                verifyPoint = await LoginAction.Instance.UserNameMenuGetText(10, fullname);
                verifyPoints.Add(summaryTC = "Verify User Name '" + fullname + "' menu is shown after login successfully", verifyPoint);
                ExtReportResult(verifyPoint, summaryTC);
                #endregion

                #region Logout test
                await LoginAction.Instance.ClicklogOut(10);

                verifyPoint = await LoginAction.Instance.IsLoginButtonShown(5);
                verifyPoints.Add(summaryTC = "Verify Login button (Dang Nhap) is displayed after logout Website", verifyPoint);
                ExtReportResult(verifyPoint, summaryTC);
                #endregion

                // Delete video record
                await DeleteFolderRecordVideoAsync();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    ExtReportResult(false, "Something wrong! Please check console log." + "<br/>" + exception);
                    //driver.Dispose();
                    Assert.Inconclusive("Something wrong! Please check console log.");
                }
            #endregion
        }
    }
}
