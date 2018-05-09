using System.Threading.Tasks;

namespace Bolt.Client
{
    /// <summary>
    /// Custom events that can be attached to the lifetime of <see cref="IProxy"/> object.
    /// </summary>
    public interface IProxyEvents
    {
        /// <summary>
        /// Called when the state of proxy is changed to <see cref="ProxyState.Open"/>. Can be used to execute post-open actions.
        /// If the open callback fails the proxy can transition either to <see cref="ProxyState.Closed"/> or <see cref="ProxyState.Default"/> state
        /// based on the result of <see cref="IErrorHandling"/> call.
        /// </summary>
        /// <param name="proxy">The instance of the proxy.</param>
        /// <returns>The pending task.</returns>
        Task OnProxyOpenedAsync(IProxy proxy);

        /// <summary>
        /// Called when the state of proxy is changed to <see cref="ProxyState.Closed"/>.
        /// If the close callback fails the proxy will always transition to <see cref="ProxyState.Closed"/> state.
        /// </summary>
        /// <param name="proxy">The instance of the proxy.</param>
        /// <returns>The pending task.</returns>
        Task OnProxyClosedAsync(IProxy proxy);

        /// <summary>
        /// If the proxy is opened and server returns error indicating that the proxy should be recovered then the proxy transitions to <see cref="ProxyState.Default"/> state
        /// and this callback is called.
        /// </summary>
        /// <param name="proxy">The instance of the proxy.</param>
        /// <returns>The pending task.</returns>
        Task OnProxyDefaultedAsync(IProxy proxy);
    }
}
