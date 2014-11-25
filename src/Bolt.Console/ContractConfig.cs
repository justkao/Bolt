using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bolt.Generators;
using Newtonsoft.Json;

namespace Bolt.Console
{
    public class ContractConfig
    {
        [JsonIgnore]
        private List<Assembly> _assemblies = new List<Assembly>();

        [JsonIgnore]
        private ContractDefinition _contractDefinition;

        [JsonProperty(Required = Required.Always)]
        public string Contract { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<string> Assemblies { get; set; }

        public string Output { get; set; }

        public string ParametersBase { get; set; }

        public List<string> Excluded { get; set; }

        public ClientConfig Client { get; set; }

        public ServerConfig Server { get; set; }

        [JsonIgnore]
        public RootConfig Parent { get; set; }

        [JsonIgnore]
        public ContractDefinition ContractDefinition
        {
            get
            {
                if (_contractDefinition == null)
                {
                    _contractDefinition = GetContractDefinition();
                }

                return _contractDefinition;
            }
        }

        public void Generate()
        {
            if (Client != null)
            {
                Client.Parent = this;
            }

            if (Server != null)
            {
                Server.Parent = this;
            }

            ContractDefinition definition = ContractDefinition;
            string output = PathHelpers.GetOutput(Parent.OutputDirectory, Output, ContractDefinition.Name + ".Contract.Designer.cs");
            DocumentGenerator document = Parent.GetDocument(output);
            document.Formatter.Assemblies = _assemblies;

            document.Add(new ContractGenerator()
                             {
                                 ContractDefinition = definition
                             });

            document.Add(new ContractDescriptorGenerator()
                             {
                                 ContractDefinition = definition
                             });


            ContractExecution execution = new ContractExecution(definition, Path.GetDirectoryName(output));
            if (Client != null)
            {
                Client.Execute(execution);
            }

            if (Server != null)
            {
                Server.Execute(execution);
            }
        }

        private ContractDefinition GetContractDefinition()
        {
            foreach (string assembly in Assemblies)
            {
                _assemblies.Add(Assembly.Load(File.ReadAllBytes(assembly)));
            }

            Type type = TypeHelper.GetTypeOrThrow(Contract);
            List<Type> excluded = new List<Type>();

            if (Excluded != null && Excluded.Any())
            {
                excluded = Excluded.Select(TypeHelper.GetTypeOrThrow).ToList();
            }

            return new ContractDefinition(type, excluded.ToArray())
            {
                ParametersBase = string.IsNullOrEmpty(ParametersBase) ? null : TypeHelper.GetTypeOrThrow(ParametersBase)
            };
        }

    }
}
