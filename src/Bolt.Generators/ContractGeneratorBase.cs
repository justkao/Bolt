using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bolt.Generators
{
    public abstract class ContractGeneratorBase : GeneratorBase
    {
        private IMetadataProvider _metadataProvider;
        private ContractDefinition _contract;
        private ClassDescriptor _descriptor;

        protected ContractGeneratorBase()
        {
        }

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

        public ContractDefinition ContractDefinition
        {
            get
            {
                if (_contract == null)
                {
                    throw new InvalidOperationException("ContractDefinition definition is not initialized.");
                }

                return _contract;
            }

            set
            {
                _contract = value;

                if (value != null)
                {
                    AddUsings(ContractDefinition.Namespace);
                }
            }
        }

        public ClassDescriptor ContractDescriptor
        {
            get
            {
                if (_descriptor != null)
                {
                    return _descriptor;
                }

                return CreateDefaultDescriptor();
            }

            set
            {
                _descriptor = value;
            }
        }

        protected virtual ClassDescriptor CreateDefaultDescriptor()
        {
            return MetadataProvider.GetContractDescriptor(ContractDefinition);
        }

        public virtual bool ShouldBeAsync(MethodInfo method, bool force)
        {
            if (method.IsAsync())
            {
                return false;
            }

            List<MethodInfo> methods = ContractDefinition.GetEffectiveMethods(method.DeclaringType).ToList();

            if (force)
            {
                return methods.All(m => m.Name != method.Name + "Async");
            }

            return method.CustomAttributes.FirstOrDefault(a => a.GetType().Name == BoltConstants.Core.AsyncOperationAttribute.Name) != null && methods.All(m => m.Name != method.Name + "Async");
        }

        public virtual ClassGenerator CreateClassGenerator(ClassDescriptor descriptor)
        {
            return new ClassGenerator(descriptor, Output, Formatter, IntendProvider);
        }

        public T CreateEx<T>() where T : ContractGeneratorBase, new()
        {
            return new T
            {
                Output = Output,
                Formatter = Formatter,
                IntendProvider = IntendProvider,
                MetadataProvider = MetadataProvider,
                ContractDefinition = ContractDefinition
            };
        }
    }
}