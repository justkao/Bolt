using System.Collections.Generic;
using System.Reflection;

namespace Bolt
{
    /// <summary>
    /// Base class for server and client action context.
    /// </summary>
    public abstract class ActionContextBase
    {
        private IDictionary<object, object> _items;

        public ActionDescriptor Action { get; set; }

        public IDictionary<object, object> Items
        {
            get { return _items ?? (_items = new Dictionary<object, object>()); }
            set { _items = value; }
        }
    }
}
