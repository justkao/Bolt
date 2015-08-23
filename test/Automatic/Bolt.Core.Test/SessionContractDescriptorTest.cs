using System;
using Bolt.Metadata;
using Xunit;

namespace Bolt.Core.Test
{
    public class SessionContractDescriptorTest
    {
        public ISessionContractMetadataProvider Provider = new SessionContractMetadataProvider();

        [Fact]
        public void Resolve_Ok()
        {
            Assert.NotNull(Provider.Resolve<IWithoutAttributes>());
            Assert.Equal(Provider.Resolve<IWithoutAttributes>().Contract, typeof(IWithoutAttributes));
        }

        [Fact]
        public void No_Attributes_EnsureOk()
        {
            Assert.NotNull(Provider.Resolve<IWithoutAttributes>().InitSession);
            Assert.NotNull(Provider.Resolve<IWithoutAttributes>().DestroySession);
        }

        [Fact]
        public void With_Attributes_EnsureOk()
        {
            Assert.NotNull(Provider.Resolve<IWithAttributes>().InitSession);
            Assert.NotNull(Provider.Resolve<IWithAttributes>().DestroySession);

            Assert.Equal(nameof(IWithAttributes.InitMySession),Provider.Resolve<IWithAttributes>().InitSession.Name);
            Assert.Equal(nameof(IWithAttributes.DestroyMySession), Provider.Resolve<IWithAttributes>().DestroySession.Name);
        }

        [Fact]
        public void With_CustomAttributes_EnsureOk()
        {
            Assert.NotNull(Provider.Resolve<IWithCustomAttributes>().InitSession);
            Assert.NotNull(Provider.Resolve<IWithCustomAttributes>().DestroySession);

            Assert.Equal(nameof(IWithCustomAttributes.InitMySession), Provider.Resolve<IWithCustomAttributes>().InitSession.Name);
            Assert.Equal(nameof(IWithCustomAttributes.DestroyMySession), Provider.Resolve<IWithCustomAttributes>().DestroySession.Name);
        }

        private interface IWithoutAttributes
        {
        }

        private interface IWithAttributes
        {
            [Bolt.InitSession]
            void InitMySession();

            [Bolt.DestroySession]
            void DestroyMySession();
        }

        private class InitSessionAttribute : Attribute
        {
            
        }

        private class DestroySessionAttribute : Attribute
        {

        }

        private interface IWithCustomAttributes
        {
            [InitSession]
            void InitMySession();

            [DestroySession]
            void DestroyMySession();
        }
    }
}
