
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

        public bool UseAspNet5 { get; set; }

        public override void Generate(object context)
        {
            AddUsings(ServerGenerator.BoltServerNamespace, "Owin");

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
                "public static IAppBuilder Use{0}(this IAppBuilder app, {1} instance, ServerConfiguration configuration = null)",
                ContractDefinition.Name,
                ContractDefinition.Root.FullName);
            using (WithBlock())
            {
                WriteLine("return app.Use{0}(new StaticInstanceProvider(instance), configuration);", ContractDefinition.Name);
            }
            WriteLine();

            WriteLine(
                "public static IAppBuilder Use{0}<TImplementation>(this IAppBuilder app, ServerConfiguration configuration = null) where TImplementation: {1}, new()",
                ContractDefinition.Name,
                ContractDefinition.Root.FullName);
            using (WithBlock())
            {
                WriteLine("return app.Use{0}(new InstanceProvider<TImplementation>(), configuration);", ContractDefinition.Name);
            }
            WriteLine();

            MethodInfo initSession = ContractDefinition.GetInitSessionMethod();
            MethodInfo closeSession = ContractDefinition.GetCloseSessionMethod();
            if (initSession != null && closeSession != null)
            {
                WriteLine(
                    "public static IAppBuilder UseStateFull{0}<TImplementation>(this IAppBuilder app, string sessionHeader = null, TimeSpan? sessionTimeout = null, ServerConfiguration configuration = null) where TImplementation: {1}, new()",
                    ContractDefinition.Name,
                    ContractDefinition.Root.FullName);
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
                        "return app.Use{0}(new {1}<TImplementation>(initSessionAction, closeSessionAction, sessionHeader ?? app.GetBolt().Configuration.SessionHeader, sessionTimeout ?? app.GetBolt().Configuration.StateFullInstanceLifetime), configuration);",
                        ContractDefinition.Name,
                        StateFullInstanceProviderBase);
                }
                WriteLine();
            }

            WriteLine(
                "public static IAppBuilder UseStateFull{0}<TImplementation>(this IAppBuilder app, ActionDescriptor initInstanceAction, ActionDescriptor releaseInstanceAction, string sessionHeader = null, TimeSpan? sessionTimeout = null, ServerConfiguration configuration = null) where TImplementation: {1}, new()",
                ContractDefinition.Name,
                ContractDefinition.Root.FullName);
            using (WithBlock())
            {
                WriteLine(
                    "return app.Use{0}(new {1}<TImplementation>(initInstanceAction, releaseInstanceAction, sessionHeader ?? app.GetBolt().Configuration.SessionHeader, sessionTimeout ?? app.GetBolt().Configuration.StateFullInstanceLifetime), configuration);",
                    ContractDefinition.Name,
                    StateFullInstanceProviderBase);
            }

            WriteLine();

            WriteLine(
                "public static IAppBuilder Use{0}(this IAppBuilder app, IInstanceProvider instanceProvider, ServerConfiguration configuration = null)",
                ContractDefinition.Name);
            using (WithBlock())
            {
                WriteLine("var boltExecutor = app.GetBolt();");
                WriteLine("var invoker = new {0}();", ContractInvoker.FullName);
                WriteLine("invoker.Init(configuration ?? boltExecutor.Configuration);");
                WriteLine("invoker.InstanceProvider = instanceProvider;");
                WriteLine("boltExecutor.Add(invoker);");
                WriteLine();
                WriteLine("return app;");
            }
        }

        protected override ClassDescriptor CreateDefaultDescriptor()
        {
            return new ClassDescriptor(ContractInvoker.Name + "Extensions", ServerGenerator.BoltServerNamespace);
        }
    }
}