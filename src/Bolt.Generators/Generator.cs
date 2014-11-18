using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Bolt.Client;
using Bolt.Server;

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

        public static Generator Create()
        {
            return new Generator();
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
                    WriteLine("Execution of '{0}' generator failed with error '{1}'", generatorBase.GetType().Name, e.ToString());
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

        public Generator All(ContractDefinition definition, bool statefull = false)
        {
            Contract(definition);
            Client(definition, null, statefull);
            Server(definition);

            return this;
        }

        public Generator Async(ContractDefinition definition, bool force = false)
        {
            return Add(new InterfaceGenerator(Output, Formatter, IntendProvider) { Contract = definition, ForceAsync = force });
        }

        public Generator Contract(ContractDefinition definition, string baseClass = null)
        {
            return Add(new ContractGenerator(Output, Formatter, IntendProvider)
            {
                Contract = definition,
                BaseClass = baseClass
            });
        }

        public Generator Server(ContractDefinition definition, string ns = null)
        {
            AddUsings(typeof(Executor).Namespace);

            return Add(new ServerGenerator(Output, Formatter, IntendProvider)
            {
                Contract = definition,
                ServerNamespace = ns,
            });
        }

        public Generator Client(ContractDefinition definition, string ns = null, bool statefull = false, bool forceAsync = false, string className = null)
        {
            AddUsings(typeof(Channel).Namespace);

            return Client(definition, ns, statefull ? FormatType<StatefullChannel>() : FormatType<Channel>(), forceAsync, className);
        }

        public Generator Client(ContractDefinition definition, string ns = null, string baseClass = null, bool forceAsync = false, string className = null)
        {
            return Add(new ClientGenerator(Output, Formatter, IntendProvider)
            {
                ClientNamespace = ns,
                BaseClass = baseClass,
                Contract = definition,
                ForceAsync = forceAsync,
                ClassName = className
            });
        }

        private Generator Add(ContractGeneratorBase generator)
        {
            generator.MetadataProvider = MetadataProvider;
            Formatter.AddNamespace(generator.Contract.RootContract.Namespace);
            _contractGenerator.Add(generator);
            return this;
        }
    }
}