using Bolt.Generators;

namespace Bolt.Server.IntegrationTest.Core
{
    public class UserCodeGenerator : IUserCodeGenerator
    {
        public void Generate(ClassGenerator generator, object context)
        {
            generator.WriteLine(
                "// useless comment added by user generator - '{0}', Context - '{1}'",
                GetType().FullName,
                context);

            generator.WriteLine();
        }
    }
}
