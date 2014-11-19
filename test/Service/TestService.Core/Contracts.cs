using Bolt;

namespace TestService.Core
{
    public static class Contracts
    {
        public static readonly ContractDefinition PersonRepository = new ContractDefinition(typeof(IPersonRepository));
    }
}
