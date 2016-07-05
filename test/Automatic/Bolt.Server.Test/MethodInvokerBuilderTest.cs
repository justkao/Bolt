using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Bolt.Server.Internal;

namespace Bolt.Server.Test
{
    public class MethodInvokerBuilderTest
    {
        [Fact]
        public void BuildVoidMethod_NoArguments()
        {
            MethodInfo method = GetMethod(nameof(IDummyInterface.VoidMethod_NoArguments));

            var lambda = MethodInvokerBuilder.Build(typeof (IDummyInterface), method);

            Moq.Mock<IDummyInterface> target = new Moq.Mock<IDummyInterface>();
            target.Setup(v => v.VoidMethod_NoArguments()).Verifiable();

            lambda(target.Object, null);

            target.Verify();
        }

        [Fact]
        public void BuildFunc_NoArguments()
        {
            MethodInfo method = GetMethod(nameof(IDummyInterface.Func_NoArguments));

            var lambda = MethodInvokerBuilder.Build(typeof(IDummyInterface), method);

            Moq.Mock<IDummyInterface> target = new Moq.Mock<IDummyInterface>();
            target.Setup(v => v.Func_NoArguments()).Returns(10);
            Assert.Equal(10, lambda(target.Object, null));
        }

        [Fact]
        public void BuildFunc_Arguments()
        {
            MethodInfo method = GetMethod(nameof(IDummyInterface.Func_Arguments), typeof (int), typeof (string));

            var lambda = MethodInvokerBuilder.Build(typeof(IDummyInterface), method);

            Moq.Mock<IDummyInterface> target = new Moq.Mock<IDummyInterface>();
            target.Setup(v => v.Func_Arguments(10, "val")).Returns(10);
            Assert.Equal(10, lambda(target.Object, new object[] {10, "val"}));
        }

        [Fact]
        public void BuildVoidMethod_Arguments()
        {
            MethodInfo method = GetMethod(nameof(IDummyInterface.VoidMethod_Arguments), typeof (int), typeof (string));

            var lambda = MethodInvokerBuilder.Build(typeof(IDummyInterface), method);

            Moq.Mock<IDummyInterface> target = new Moq.Mock<IDummyInterface>();
            target.Setup(v => v.VoidMethod_Arguments(10, "val"));
            Assert.Equal(null, lambda(target.Object, new object[] { 10, "val" }));
        }

        [Fact]
        public void BuildTaskResultProvider()
        {
            var provider = MethodInvokerBuilder.BuildTaskResultProvider(typeof (int));

            Assert.Equal(10, provider(Task.FromResult(10)));
            Assert.Equal(11, provider(Task.FromResult(11)));
        }

        private static MethodInfo GetMethod(string name, params Type[] parameters)
        {
            return typeof(IDummyInterface).GetTypeInfo().GetMethod(name, parameters);
        }

        public interface IDummyInterface
        {
            void VoidMethod_NoArguments();

            int Func_NoArguments();

            int Func_Arguments(int param1, string param2);

            void VoidMethod_Arguments(int param1, string param2);
        }
    }
}
