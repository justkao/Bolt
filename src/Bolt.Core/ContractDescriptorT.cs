using System.Linq;
using System.Reflection;

namespace Bolt
{
    public class ContractDescriptor<T> where T : ContractDescriptor
    {
        private static readonly T Descriptor;

        static ContractDescriptor()
        {
            FieldInfo defaultValue = typeof(T).GetTypeInfo().DeclaredFields.First(m => m.IsStatic && m.Name == "Default");
            Descriptor = (T)defaultValue.GetValue(null);
        }

        public static T Instance
        {
            get
            {
                return Descriptor;
            }
        }
    }
}
