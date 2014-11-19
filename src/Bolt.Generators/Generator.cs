using Bolt.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bolt.Generators
{
    public class Generator : ContractGeneratorBase
    {
        private readonly List<ContractGeneratorBase> _contractGenerator = new List<ContractGeneratorBase>();

        public Generator()
            : this(new StringWriter(), new TypeFormatter(), new IntendProvider())
        {
        }

        public Generator(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
            : base(output, formatter, intendProvider)
        {
        }

        public static Generator Create(ContractDefinition contract = null)
        {
            return new Generator()
            {
                ContractDefinition = contract
            };
        }

        public string GetResult()
        {
            Generate();
            return Output.GetStringBuilder().ToString();
        }

        public override void Generate()
        {
            foreach (ContractGeneratorBase generatorBase in _contractGenerator)
            {
                try
                {
                    generatorBase.Generate();
                }
                catch (Exception e)
                {
                    WriteLine("/*");
                    WriteLine("Execution of '{0}' generator failed with error '{1}'", generatorBase.GetType().Name,
                        e.ToString());
                    WriteLine("*/");
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (string ns in Formatter.GetNamespaces())
            {
                sb.AppendFormat("using {0};\n", ns);
            }
            sb.AppendLine();

            Output.GetStringBuilder().Insert(0, sb.ToString());
        }

        public Generator Async(bool force = false)
        {
            return Add(new InterfaceGenerator(Output, Formatter, IntendProvider)
            {
                ContractDefinition = ContractDefinition,
                ForceAsync = force
            });
        }

        public Generator Async(ContractDefinition definition, bool force = false)
        {
            return Add(new InterfaceGenerator(Output, Formatter, IntendProvider)
            {
                ContractDefinition = definition ?? ContractDefinition,
                ForceAsync = force
            });
        }

        public Generator Contract(string baseClass = null)
        {
            return Add(new ContractGenerator(Output, Formatter, IntendProvider)
            {
                ContractDefinition = ContractDefinition,
                BaseClass = baseClass
            });
        }

        public Generator Contract(ContractDefinition definition, string baseClass = null)
        {
            return Add(new ContractGenerator(Output, Formatter, IntendProvider)
            {
                ContractDefinition = definition,
                BaseClass = baseClass
            });
        }

        public Generator Server(ContractDefinition definition, string ns = null)
        {
            return Add(new ServerGenerator(Output, Formatter, IntendProvider)
            {
                ContractDefinition = definition,
                ServerNamespace = ns,
            });
        }

        public Generator Server(string ns = null)
        {
            return Add(new ServerGenerator(Output, Formatter, IntendProvider)
            {
                ContractDefinition = ContractDefinition,
                ServerNamespace = ns,
            });
        }

        public Generator StatelessClient(string clientNamespace = null, bool forceAsync = false, string className = null)
        {
            AddUsings(typeof(Channel).Namespace);

            return Client(ContractDefinition, clientNamespace, FormatType<Channel>(), forceAsync, className);
        }

        public Generator StatelessClient(ContractDefinition definition, string clientNamespace = null, bool forceAsync = false, string className = null)
        {
            AddUsings(typeof(Channel).Namespace);

            return Client(definition, clientNamespace, FormatType<Channel>(), forceAsync, className);
        }

        public Generator StatefullClient(string clientNamespace = null, bool forceAsync = false, string className = null)
        {
            AddUsings(typeof(Channel).Namespace);

            return Client(ContractDefinition, clientNamespace, FormatType<StatefullChannel>(), forceAsync, className);
        }

        public Generator StatefullClient(ContractDefinition definition, string clientNamespace = null, bool forceAsync = false, string className = null)
        {
            AddUsings(typeof(Channel).Namespace);

            return Client(definition, clientNamespace, FormatType<StatefullChannel>(), forceAsync, className);
        }

        public Generator Client(string clientNamespace = null, string baseClass = null, bool forceAsync = false, string className = null)
        {
            return Client(ContractDefinition, clientNamespace, baseClass, forceAsync, className);
        }

        public Generator Client(ContractDefinition definition, string clientNamespace = null, string baseClass = null, bool forceAsync = false, string className = null)
        {
            return Add(new ClientGenerator(Output, Formatter, IntendProvider)
            {
                ClientNamespace = clientNamespace,
                BaseClass = baseClass,
                ContractDefinition = definition,
                ForceAsync = forceAsync,
                ClassName = className
            });
        }

        private Generator Add(ContractGeneratorBase generator)
        {
            generator.MetadataProvider = MetadataProvider;
            Formatter.AddNamespace(generator.ContractDefinition.Namespace);
            _contractGenerator.Add(generator);
            return this;
        }
    }
}