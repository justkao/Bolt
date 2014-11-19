using System;
using System.IO;
using System.Reflection;

namespace Bolt.Generators
{
    public abstract class ContractGeneratorBase : GeneratorBase
    {
        private IMetadataProvider _metadataProvider;
        private ContractDefinition _contract;

        protected ContractGeneratorBase(StringWriter output, TypeFormatter formatter, IntendProvider intendProvider)
            : base(output, formatter, intendProvider)
        {
            MetadataProvider = new MetadataProvider();
        }

        public IMetadataProvider MetadataProvider
        {
            get
            {
                if (_metadataProvider == null)
                {
                    throw new InvalidOperationException("Metadata provider is not initialized.");
                }

                return _metadataProvider;
            }

            set { _metadataProvider = value; }
        }

        public ContractDefinition Contract
        {
            get
            {
                if (_contract == null)
                {
                    throw new InvalidOperationException("Contract definition is not initialized.");
                }

                return _contract;
            }

            set
            {
                _contract = value;

                if (value != null)
                {
                    value.Validate();

                    AddUsings(typeof(ContractDefinition).Namespace);
                    AddUsings(Contract.RootContract.Namespace);
                }
            }
        }

        public abstract void Generate();

        protected virtual string GetMethodDescriptorReference(MethodDescriptor descriptor, MethodInfo info)
        {
            TypeDescriptor typeDescriptor = MetadataProvider.GetTypeDescriptor(info.DeclaringType);
            return typeDescriptor.FullName + "." + descriptor.Method;
        }
    }
}