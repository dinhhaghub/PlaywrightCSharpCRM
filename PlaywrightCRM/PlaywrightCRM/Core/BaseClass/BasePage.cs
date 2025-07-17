using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightCRM.Core.BaseClass
{
    internal abstract class BasePage<S, M> : BaseSingletonThreadSafeNestedConstructors<S>
        where M : BasePageElementMap, new()
        where S : BasePage<S, M>
    {
        protected M Map
        {
            get { return new M(); }
        }
    }

    internal abstract class BasePage<S, M, V> : BasePage<S, M>
        where M : BasePageElementMap, new()
        where S : BasePage<S, M, V>
        where V : BasePageValidator<S, M, V>, new()
    {
        internal V Validate()
        { return new V(); }
    }
}
