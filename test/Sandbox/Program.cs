
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization.Formatters;

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
            Exception e = new InvalidOperationException("Test");

            try
            {
                throw e;
            }
            catch (Exception ex)
            {
                e = ex;
            }

            var resul = JsonConvert.SerializeObject(e, new JsonSerializerSettings()
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.All
            });
            e = (Exception)JsonConvert.DeserializeObject(resul, new JsonSerializerSettings()
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.All
            });

        }
    }
}
