using Bolt;

namespace TestService.Contracts
{
    public static class Contracts
    {
        public static readonly ContractDefinition PersonRepository =
            new ContractDefinition(typeof(IPersonRepository))
            {
                Recursive = true,
            };

        public static readonly ContractDefinition ServerState =
            new ContractDefinition(typeof(IServeState))
            {
                Recursive = true,
            };
    }
}
