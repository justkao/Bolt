namespace Bolt
{
    public interface IEndpointProvider
    {
        MethodDescriptor GetEndpoint(MethodDescriptor descriptor);
    }
}
