using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bolt.Tools.Generators
{
    public abstract class ContractGeneratorBase : GeneratorBase
    {
        private ContractDefinition _contract;
        private ClassDescriptor _descriptor;

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

                return CreateDefaultClassDescriptor();
            }

            set
            {
                _descriptor = value;
            }
        }

        protected virtual ClassDescriptor CreateDefaultClassDescriptor()
        {
            return new ClassDescriptor(ContractDefinition.Name, ContractDefinition.Namespace);
        }

        public virtual bool ShouldBeSync(MethodInfo method, bool force)
        {
            if (!method.IsAsync())
            {
                return false;
            }

            string methodName = method.Name.TrimEnd(AsyncSuffix);

            List<MethodInfo> methods = ContractDefinition.GetEffectiveMethods(method.DeclaringType).ToList();

            if (force)
            {
                return methods.All(m => m.Name != methodName);
            }

            if (!method.CustomAttributes.Any())
            {
                return false;
            }

            var found = method.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == BoltConstants.Core.SyncOperationAttribute.Name);
            if (found == null)
            {
                return false;
            }

            return methods.All(m => m.Name != methodName);
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
                return methods.All(m => m.Name != method.Name + AsyncSuffix);
            }

            if (!method.CustomAttributes.Any())
            {
                return false;
            }

            var found = method.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == BoltConstants.Core.AsyncOperationAttribute.Name);
            if (found == null)
            {
                return false;
            }

            return methods.All(m => m.Name != method.Name + AsyncSuffix);
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
                ContractDefinition = ContractDefinition
            };
        }
    }
}