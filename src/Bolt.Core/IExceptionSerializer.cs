using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization.Formatters;

namespace Bolt
{
    public interface IExceptionSerializer
    {
        string Serialize(Exception exception);

        Exception Deserialize(string exception);
    }

    public class ExceptionSerializer : IExceptionSerializer
    {
        private readonly JsonSerializerSettings _exceptionSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new MyContractResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ObjectCreationHandling = ObjectCreationHandling.Auto,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public string Serialize(Exception exception)
        {
            string raw = JsonConvert.SerializeObject(exception, _exceptionSerializerSettings);
            return raw;
        }

        public Exception Deserialize(string exception)
        {
            return JsonConvert.DeserializeObject<Exception>(exception, _exceptionSerializerSettings);
        }

        private class MyContractResolver : DefaultContractResolver
        {
            private readonly List<MemberInfo> _ignoredMembers = new List<MemberInfo>();

            public MyContractResolver()
                : base(true)
            {
                _ignoredMembers.Add(typeof(Exception).GetTypeInfo().GetDeclaredProperty("TargetSite"));
            }

            public override JsonContract ResolveContract(Type type)
            {
                return new JsonObjectContract(typeof(Exception))
                {
                    
                };

                return base.ResolveContract(type);
            }

            protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
            {
                return base.CreateDictionaryContract(objectType);
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
                }

                return property;
            }
        }
    }
}
