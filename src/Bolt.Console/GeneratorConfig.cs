using System;

using Bolt.Generators;

using Newtonsoft.Json;

namespace Bolt.Console
{
    public class GeneratorConfig
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }

        [JsonIgnore]
        public RootConfig Parent { get; set; }

        public IUserGenerator GetGenerator()
        {
            return (IUserGenerator)Activator.CreateInstance(Parent.AssemblyCache.GetType(Type));
        }
    }
}