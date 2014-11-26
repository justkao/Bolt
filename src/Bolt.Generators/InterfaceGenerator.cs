using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bolt.Generators
{
    public class InterfaceGenerator : ContractGeneratorBase
    {
        private readonly List<Type> _generatedInterfaces = new List<Type>();

        private readonly List<string> _generatedAsyncInterfaces = new List<string>();

        public InterfaceGenerator()
        {
        }

        public InterfaceGenerator(StringWriter output, TypeFormatter formatter, IntendProvider provider)
            : base(output, formatter, provider)
        {
        }

        public event EventHandler Generated;

        public IEnumerable<string> GeneratedAsyncInterfaces
        {
            get { return _generatedAsyncInterfaces; }
        }

        public bool ForceAsync { get; set; }

        public List<string> ExcludedInterfaces { get; set; }

        public override void Generate()
        {
            bool hasAsyncInterfaces = ContractDefinition.GetEffectiveContracts().Any(ShouldHaveAsyncMethods);

            foreach (var iface in ContractDefinition.GetEffectiveContracts().Except(new[] { ContractDefinition.Root }))
            {
                if (hasAsyncInterfaces)
                {
                    GenerateAsyncInterface(iface);
                }
            }

            GenerateAsyncInterface(ContractDefinition.Root);

            if (Generated != null)
            {
                Generated(this, EventArgs.Empty);
            }
        }

        private void GenerateAsyncInterface(Type iface)
        {
            if (ExcludedInterfaces != null)
            {
                if (ExcludedInterfaces.Contains(iface.FullName) || ExcludedInterfaces.Contains(iface.FullName + "Async"))
                {
                    return;
                }
            }

            if (_generatedInterfaces.Contains(iface))
            {
                return;
            }

            if (!ContractDefinition.GetEffectiveContracts(iface).Concat(new[] { iface }).Any(ShouldHaveAsyncMethods))
            {
                return;
            }

            _generatedInterfaces.Add(iface);
            string name = iface.Name + "Async";
            _generatedAsyncInterfaces.Add(name);

            List<string> asyncBase = ContractDefinition.GetEffectiveContracts(iface).Where(ShouldHaveAsyncMethods).Where(
                i =>
                {
                    if (ExcludedInterfaces != null)
                    {
                        if (ExcludedInterfaces.Contains(i.FullName))
                        {
                            return false;
                        }
                    }
                    return true;
                }).Select(t => FormatType(t) + "Async").ToList();

            asyncBase.Insert(0, FormatType(iface));

            ClassGenerator classGenerator = CreateClassGenerator(new ClassDescriptor(name, iface.Namespace, asyncBase.ToArray()) { IsInterface = true });
            classGenerator.GenerateClass((g) =>
            {
                List<MethodInfo> methods = (from m in ContractDefinition.GetEffectiveMethods(iface)
                                            where ShouldBeAsync(m, ForceAsync)
                                            select m).ToList();


                foreach (MethodInfo method in methods)
                {
                    g.WriteLine(FormatMethodDeclaration(method, true) + ";");
                    if (!Equals(method, methods.Last()))
                    {
                        g.WriteLine();
                    }
                }
            });
        }

        private bool ShouldHaveAsyncMethods(Type iface)
        {
            return ContractDefinition.GetEffectiveMethods(iface).Any(m => ShouldBeAsync(m, ForceAsync));
        }
    }
}