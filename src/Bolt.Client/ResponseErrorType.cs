namespace Bolt.Client
{
    public enum ResponseErrorType
    {
        None,
        Serialization,
        Deserialization,
        Communication,
        Client,
        Server
    }
}