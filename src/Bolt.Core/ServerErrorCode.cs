namespace Bolt
{
    public enum ServerErrorCode
    {
        Unknown = 0,
        Serialization = 1,
        Deserialization = 2,
        ActionNotFound = 3,
        ContractNotFound = 4,
        ActionNotImplemented = 5,
    }
}
