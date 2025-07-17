using AventStack.ExtentReports;
using PlaywrightCRM.Core.BaseTestCase;
using PlaywrightCRM.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightCRM.Tests.Features_Testing.Regression_Testing
{
    [TestFixture, Order(2)]
    internal class LeadTest : BaseTestCase
    {
        #region Declare global variables
        [Obsolete]
        readonly ExtentReports rep = ExtReportgetInstance();
        private string? summaryTC;
        private static readonly string dateTimeNow = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Replace("/", "").Replace(" ", "").Replace(":", "");
        private static string? url = null;
        #endregion

        [Test, Category("Regression Tests")]
        public async Task TC001_Create_search_delete_leadAsync()
        {
            #region Declare variables
            url = LoginPage.GetConfigFile().BaseUrl;
            string username = LoginPage.GetConfigFile().username;
            string password = LoginPage.GetConfigFile().password;
            #endregion

            #region Workflow scenario
            //Verify steps, if getting issue show warning message
            test = rep.CreateTest("Connext - Lead Test 001 - Create search delete Lead");
            try
            {
                // Go to website
                await LoginAction.Instance.LoginSite(10, url, username, password);

                // Click on Home menu
                await LeadAction.Instance.ClickTitleMenuAsync(10, "Home Menu");

                // Go to Lead page
                await LeadAction.Instance.ClickLeadMenuAsync(10);
                Thread.Sleep(2000);

                // Delete video record
                await DeleteFolderRecordVideoAsync();
            }
            catch (Exception exception)
            {
                // Print exception
                Console.WriteLine(exception);

                // Warning
                ExtReportResult(false, "Something wrong! Please check console log." + "<br/>" + exception);
                Assert.Inconclusive("Something wrong! Please check console log.");
            }
            #endregion
        }
    }
}
