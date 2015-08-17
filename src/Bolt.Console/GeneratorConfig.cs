using System.Collections.Generic;
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

        public Dictionary<string, string> Properties { get; set; }

        [JsonIgnore]
        public RootConfig Parent { get; set; }

        public IUserCodeGenerator GetGenerator()
        {
            return UserCodeGeneratorExtensions.Activate(Parent.AssemblyCache.GetType(Type), Properties);
        }
    }
}