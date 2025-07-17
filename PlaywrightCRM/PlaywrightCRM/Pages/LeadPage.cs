using Microsoft.Playwright;
using PlaywrightCRM.Core.BaseClass;
using PlaywrightCRM.Core.PlaywrightDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightCRM.Pages
{
    internal class LeadPage : BasePageElementMap
    {
        // Variables declaration
        internal static ILocator? _leadMenu;
        internal static ILocator? _titleMenu;

        // Initiate the ILocator for elements
        public async Task HighlightElement(ILocator locator, string color = "blue", int thickness = 3)
        {
            await locator.EvaluateAsync($@"(e) => {{e.style.border = '{thickness}px solid {color}';}}"); Thread.Sleep(150);
            await locator.EvaluateAsync($@"(e) => {{e.style.removeProperty('border');}}");
        }
        public ILocator LeadMenuAsync()
        {
            if (page == null)
            {
                throw new InvalidOperationException("The page instance is null. Ensure the page is properly initialized.");
            }

            _leadMenu = page.Locator("//a[.='Lead']");
            return _leadMenu;
        }
        public ILocator TitleMenuAsync(string title) 
        {
            if (page == null)
            {
                throw new InvalidOperationException("The page instance is null. Ensure the page is properly initialized.");
            }

            _titleMenu = page.Locator($"//*[@title='{title}']"); // //*[@title='" + title + "']
            return _titleMenu;
        }
    }

    internal sealed class LeadAction : BasePage<LeadAction, LeadPage>
    {
        #region Constructor  
        private LeadAction() { }
        #endregion

        #region Items Action  
        public async Task ClickLeadMenuAsync(int timeout)
        {
            if (timeout > 0)
            {
                await Map.LeadMenuAsync().WaitForAsync(new LocatorWaitForOptions { Timeout = timeout * 1000 });
            }
            await Map.LeadMenuAsync().ClickAsync();
        }
        public async Task ClickTitleMenuAsync(int timeout, string title)
        {
            if (timeout > 0)
            {
                await Map.TitleMenuAsync(title).WaitForAsync(new LocatorWaitForOptions { Timeout = timeout * 1000 });
            }
            await Map.TitleMenuAsync(title).ClickAsync();
        }
        #endregion

        #region Built-in Actions  
        #endregion
    }
}
