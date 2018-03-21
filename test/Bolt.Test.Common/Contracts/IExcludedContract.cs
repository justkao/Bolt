using System.Threading;
using System.Threading.Tasks;
using Bolt.Test.Common;

namespace Bolt.Server.IntegrationTest.Core
{
    public interface IExcludedContract
    {
        void ThisMethodShouldBeExcluded();
    }
}
