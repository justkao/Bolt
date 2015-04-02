using System.Collections.Generic;

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
            get
            {
                if (_items == null)
                {
                    _items = new Dictionary<object, object>();
                }
                return _items;
            }

            set { _items = value; }
        }
    }
}
