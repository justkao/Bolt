using System;
using System.Collections.Generic;
using System.Linq;
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
            MethodInfo method = typeof (IDummyInterface).GetTypeInfo()
                .GetRuntimeMethod(nameof(IDummyInterface.VoidMethod_NoArguments), new Type[0]);

            var lambda = MethodInvokerBuilder.Build(typeof (IDummyInterface), method);

            Moq.Mock<IDummyInterface> target = new Moq.Mock<IDummyInterface>();
            target.Setup(v => v.VoidMethod_NoArguments()).Verifiable();

            lambda(target.Object, null);

            target.Verify();
        }

        [Fact]
        public void BuildFunc_NoArguments()
        {
            MethodInfo method = typeof(IDummyInterface).GetTypeInfo()
                .GetRuntimeMethod(nameof(IDummyInterface.Func_NoArguments), new Type[0]);

            var lambda = MethodInvokerBuilder.Build(typeof(IDummyInterface), method);

            Moq.Mock<IDummyInterface> target = new Moq.Mock<IDummyInterface>();
            target.Setup(v => v.Func_NoArguments()).Returns(10);
            Assert.Equal(10, lambda(target.Object, null));
        }

        [Fact]
        public void BuildFunc_Arguments()
        {
            MethodInfo method = typeof (IDummyInterface).GetTypeInfo()
                .GetRuntimeMethod(nameof(IDummyInterface.Func_Arguments), new[] {typeof (int), typeof (string)});

            var lambda = MethodInvokerBuilder.Build(typeof(IDummyInterface), method);

            Moq.Mock<IDummyInterface> target = new Moq.Mock<IDummyInterface>();
            target.Setup(v => v.Func_Arguments(10, "val")).Returns(10);
            Assert.Equal(10, lambda(target.Object, new object[] {10, "val"}));
        }

        [Fact]
        public void BuildVoidMethod_Arguments()
        {
            MethodInfo method = typeof(IDummyInterface).GetTypeInfo()
                .GetRuntimeMethod(nameof(IDummyInterface.VoidMethod_Arguments), new[] { typeof(int), typeof(string) });

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

        public interface IDummyInterface
        {
            void VoidMethod_NoArguments();

            int Func_NoArguments();

            int Func_Arguments(int param1, string param2);

            void VoidMethod_Arguments(int param1, string param2);
        }
    }
}
