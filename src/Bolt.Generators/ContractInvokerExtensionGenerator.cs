
using System.Reflection;

namespace Bolt.Generators
{
    public class ContractInvokerExtensionGenerator : ContractGeneratorBase
    {
        public ContractInvokerExtensionGenerator()
        {
            Modifier = "public";
            StateFullInstanceProviderBase = "StateFullInstanceProvider";
        }

        public ClassDescriptor ContractInvoker { get; set; }

        public string Modifier { get; set; }

        public string StateFullInstanceProviderBase { get; set; }

        public IUserCodeGenerator UserGenerator { get; set; }

        public override void Generate(object context)
        {
            AddUsings(BoltConstants.BoltServerNamespace);

            ClassGenerator generator = CreateClassGenerator(ContractDescriptor);
            generator.Modifier = Modifier + " static";
            generator.GenerateBodyAction = GenerateBody;
            generator.UserGenerator = UserGenerator;
            generator.Generate(context);

            base.Generate(context);
        }

        private void GenerateBody(ClassGenerator g)
        {
            WriteLine(
                "public static IContractInvoker Use{0}(this {2} bolt, {1} instance)",
                ContractDefinition.Name,
                ContractDefinition.Root.FullName,
                BoltRouteHandler);
            using (WithBlock())
            {
                WriteLine(
                    "return bolt.Use{0}(new StaticInstanceProvider(instance));",
                    ContractDefinition.Name);
            }
            WriteLine();

            WriteLine(
                "public static IContractInvoker Use{0}<TImplementation>(this {2} bolt) where TImplementation: {1}, new()",
                ContractDefinition.Name,
                ContractDefinition.Root.FullName,
                BoltRouteHandler);
            using (WithBlock())
            {
                WriteLine(
                    "return bolt.Use{0}(new InstanceProvider<TImplementation>());",
                    ContractDefinition.Name);
            }
            WriteLine();

            MethodInfo initSession = ContractDefinition.GetInitSessionMethod();
            MethodInfo closeSession = ContractDefinition.GetCloseSessionMethod();
            if (initSession != null && closeSession != null)
            {
                WriteLine(
                    "public static IContractInvoker UseStateFull{0}<TImplementation>(this {2} bolt, {3} options = null) where TImplementation: {1}, new()",
                    ContractDefinition.Name,
                    ContractDefinition.Root.FullName,
                    BoltRouteHandler, 
                    BoltConstants.BoltServerOptions);
                using (WithBlock())
                {
                    WriteLine(
                        "var initSessionAction = {0}.Default.{1};",
                        MetadataProvider.GetContractDescriptor(ContractDefinition).Name,
                        MetadataProvider.GetMethodDescriptor(ContractDefinition, initSession).Name);
                    WriteLine(
                        "var closeSessionAction = {0}.Default.{1};",
                        MetadataProvider.GetContractDescriptor(ContractDefinition).Name,
                        MetadataProvider.GetMethodDescriptor(ContractDefinition, closeSession).Name);
                    WriteLine(
                        "return bolt.Use{0}(new {1}<TImplementation>(initSessionAction, closeSessionAction, options ?? bolt.Options));",
                        ContractDefinition.Name,
                        StateFullInstanceProviderBase);
                }
                WriteLine();
            }

            WriteLine(
                "public static IContractInvoker UseStateFull{0}<TImplementation>(this {2} bolt, ActionDescriptor initInstanceAction, ActionDescriptor releaseInstanceAction, {3} options = null) where TImplementation: {1}, new()",
                ContractDefinition.Name,
                ContractDefinition.Root.FullName,
                BoltRouteHandler, 
                BoltConstants.BoltServerOptions);
            using (WithBlock())
            {
                WriteLine(
                    "return bolt.Use{0}(new {1}<TImplementation>(initInstanceAction, releaseInstanceAction, options ?? bolt.Options));",
                    ContractDefinition.Name,
                    StateFullInstanceProviderBase);
            }

            WriteLine();

            WriteLine(
                "public static IContractInvoker Use{0}(this {1} bolt, IInstanceProvider instanceProvider)",
                ContractDefinition.Name,
                BoltRouteHandler);
            using (WithBlock())
            {
                WriteLine("var invoker = new {0}();", ContractInvoker.FullName);
                WriteLine("invoker.Init(bolt, instanceProvider);");
                WriteLine("bolt.Add(invoker);");
                WriteLine();
                WriteLine("return invoker;");
            }
        }

        protected override ClassDescriptor CreateDefaultDescriptor()
        {
            return new ClassDescriptor(ContractInvoker.Name + "Extensions", BoltConstants.BoltServerNamespace);
        }

        private string BoltRouteHandler
        {
            get
            {
                return BoltConstants.BoltRouteHandler;
            }
        }
    }
}