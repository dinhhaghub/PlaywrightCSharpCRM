using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightCRM.Core.BaseClass
{
    internal class BasePageValidator<S, M, V>
        where S : BasePage<S, M, V>
        where M : BasePageElementMap, new()
        where V : BasePageValidator<S, M, V>, new()
    {
        protected S pageInstance;
        internal BasePageValidator(S currentInstance) { pageInstance = currentInstance; }
        internal BasePageValidator() { }
        protected M Map
        {
            get { return new M(); }
        }
    }
}
