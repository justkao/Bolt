using System.Linq;

namespace Bolt.Server
{
    public static class ContractDescriptorExtensions
    {
        public static ActionDescriptor Find(this ContractDescriptor descriptor, string actionName)
        {
            actionName = actionName.ToLowerInvariant();

            var found = descriptor.FirstOrDefault(a => string.CompareOrdinal(a.Name.ToLowerInvariant(), actionName) == 0);
            if (found != null)
            {
                return found;
            }

            var index = actionName.IndexOf("async");
            if (index == -1)
            {
                actionName += "async";
                return descriptor.FirstOrDefault(a => string.CompareOrdinal(a.Name.ToLowerInvariant(), actionName) == 0);
            }

            actionName = actionName.Substring(0, index);
            return descriptor.FirstOrDefault(a => string.CompareOrdinal(a.Name.ToLowerInvariant(), actionName) == 0);
        }
    }
}