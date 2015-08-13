namespace Bolt
{
    public enum ClientErrorCode
    {
        ConnectionUnavailable = 0,

        ProxyNotInitialized = 1,

        InvalidDestroySessionParameters = 2,

        InvalidInitSessionParameters = 3,

        SerializeParameters  = 4,

        DeserializeResponse = 5,

        DeserializeExceptionResponse = 6
    }
}