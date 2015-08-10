namespace Bolt
{
    public static class ContractProviderExtensions
    {
        public static string GetContractName(this IContractProvider contractProvider)
        {
            return BoltFramework.GetContractName(contractProvider.Contract);
        }
    }
}