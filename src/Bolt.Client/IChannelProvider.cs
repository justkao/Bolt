namespace Bolt.Client
{
    public interface IChannelProvider
    {
        IChannel Channel { get; set; }
    }
}