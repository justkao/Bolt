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
                    AddUsings(typeof(ContractDefinition).Namespace);
                    AddUsings(ContractDefinition.Namespace);
                }
            }
        }

        public abstract void Generate();

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

            return method.GetCustomAttribute<AsyncOperationAttribute>() != null && methods.All(m => m.Name != method.Name + "Async");
        }
    }
}