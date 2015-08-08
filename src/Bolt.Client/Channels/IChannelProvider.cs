namespace Bolt.Client.Channels
{
    public interface IChannelProvider
    {
        IChannel Channel { get; }
    }
}