using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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

            using (WithNamespace(iface.Namespace))
            {
                IEnumerable<string> asyncBase = (from i in ContractDefinition.GetEffectiveContracts(iface).Where(ShouldHaveAsyncMethods)
                                                 select FormatType(i) + "Async").ToList();

                StringBuilder sb = new StringBuilder();
                foreach (string s in asyncBase)
                {
                    sb.AppendFormat("{0}, ", s);
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 2, 2);
                    sb.Insert(0, ", ");
                }

                WriteLine("public interface {0} : {1}{2}", name, FormatType(iface), sb.ToString());
                using (WithBlock())
                {
                    List<MethodInfo> methods = (from m in ContractDefinition.GetEffectiveMethods(iface)
                                                where ShouldBeAsync(m, ForceAsync)
                                                select m).ToList();


                    foreach (MethodInfo method in methods)
                    {
                        WriteLine(FormatMethodDeclaration(method, true) + ";");
                        if (method != methods.Last())
                        {
                            WriteLine();
                        }
                    }
                }
            }

            WriteLine();
        }

        private bool ShouldHaveAsyncMethods(Type iface)
        {
            return ContractDefinition.GetEffectiveMethods(iface).Any(m => ShouldBeAsync(m, ForceAsync));
        }
    }
}