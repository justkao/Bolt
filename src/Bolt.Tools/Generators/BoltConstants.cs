namespace Bolt.Tools.Generators
{
    internal static class BoltConstants
    {
        internal static class Core
        {
            public const string Namespace = "Bolt";

            public static readonly ClassDescriptor AsyncOperationAttribute = new ClassDescriptor("AsyncOperationAttribute", Namespace);

            public static readonly ClassDescriptor SyncOperationAttribute = new ClassDescriptor("SyncOperationAttribute", Namespace);
        }
    }
}
