
using Bolt;
using Bolt.Generators;

namespace Sandbox
{
    public interface ISamle2
    {
        void DoSomething2();

    }

    public interface ISamle1 : ISamle2
    {
        void DoSomething(double argument);
    }

    class Program
    {
        static void Main(string[] args)
        {
            ContractDefinition definition = new ContractDefinition(typeof(ISamle1));
            string result = Generator.Create().Contract(definition).GetResult();

        }
    }
}
