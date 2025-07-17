using Microsoft.Playwright;
using PlaywrightCRM.Core.PlaywrightDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightCRM.Core.BaseClass
{
    internal class BasePageElementMap
    {
        protected IBrowser? browser;
        protected IBrowserContext? browserContext;
        internal IPage? page;

        internal BasePageElementMap()
        {
            browser = Driver.Browser;
            page = Driver.Page;
            browserContext = Driver.BrowserContext;
        }
    }
}
