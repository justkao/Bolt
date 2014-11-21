
using Bolt;
using Bolt.Generators;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
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
            string result = new Generator() { ContractDefinition = new ContractDefinition(typeof(ISamle1)) }.StateFullClient().GetResult();

            Exception e = new InvalidOperationException("Test");
            try
            {
                throw e;
            }
            catch (Exception ex)
            {
                e = ex;
            }

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new MyContractResolver()
            };


            var resul = JsonConvert.SerializeObject(e, settings);
            e = (Exception)JsonConvert.DeserializeObject(resul, settings);
        }

        private class MyContractResolver : DefaultContractResolver
        {
            private readonly List<MemberInfo> _ignoredMembers = new List<MemberInfo>();

            public MyContractResolver()
            {
                _ignoredMembers.Add(typeof(Exception).GetTypeInfo().GetDeclaredProperty("TargetSite"));
            }

            protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
            {
                return base.CreateMemberValueProvider(member);
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);
                if (_ignoredMembers.Contains(member))
                {
                    property.Ignored = true;
                }

                if (property.PropertyName == "Message")
                {
                    property.Writable = true;
                    property.ItemConverter = new KeyValuePairConverter();
                    property.MemberConverter = new KeyValuePairConverter();
                }

                return property;
            }
        }

    }
}
