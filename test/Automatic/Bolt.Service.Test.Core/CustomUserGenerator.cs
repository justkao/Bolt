using Bolt.Generators;

namespace Bolt.Service.Test.Core
{
    public class CustomUserGenerator : IUserGenerator
    {
        public void Generate(ClassGenerator generator)
        {
            generator.WriteLine("// useless comment added by user generator - '{0}'", GetType().FullName);
            generator.WriteLine();
        }
    }
}
