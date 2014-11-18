using System.Collections.Generic;
using Bolt.Generators;

namespace Bolt.Sandbox
{
    public interface ITestInterface
    {
        List<int> GetSomethingAsync(int test, string name);

        [AsyncOperation]
        List<int> GetSomething(int test, string name);

        void DoSomething(string name, double value);
    }

    class Program
    {
        static void Main(string[] args)
        {
            var result = Generator.Create().Client(new ContractDefinition(typeof(ITestInterface)), null, true).GetResult();
        }
    }
}
