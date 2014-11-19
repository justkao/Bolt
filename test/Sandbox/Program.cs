
using System.Runtime.Serialization.Formatters;
using Bolt;
using Bolt.Generators;
using Newtonsoft.Json;
using System;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

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
            Exception e = new InvalidOperationException("Test", new Exception("inner"));
            var resul = JsonConvert.SerializeObject(e, Formatting.None, new JsonSerializerSettings()
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
                TypeNameHandling = TypeNameHandling.All 
            });
            e = (Exception)JsonConvert.DeserializeObject(resul, new JsonSerializerSettings()
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
                TypeNameHandling = TypeNameHandling.All
            });

            JsonSerializer serializer = new JsonSerializer();
            ContractDefinition definition = new ContractDefinition(typeof(ISamle1));
            string result = Generator.Create().Contract(definition).GetResult();

        }
    }
}
