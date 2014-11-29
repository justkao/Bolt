namespace Bolt.Client
{
    public interface IServerErrorProvider
    {
        ServerErrorCode? TryRead(ClientActionContext context);

        int? TryReadErrorCode(ClientActionContext context);
    }
}
