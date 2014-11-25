using System.Threading;

namespace Bolt.Client.Channels
{
    public class ActionChannel : ChannelBase
    {
        private readonly ClientActionContext _context;

        public ActionChannel(ActionChannel proxy)
            : base(proxy)
        {
            _context = proxy._context;
        }

        public ActionChannel(IRequestForwarder requestForwarder, IEndpointProvider endpointProvider, ClientActionContext context)
            : base(requestForwarder, endpointProvider)
        {
            _context = context;
        }

        protected override ClientActionContext CreateContext(ActionDescriptor actionDescriptor, CancellationToken cancellation, object parameters)
        {
            return _context;
        }
    }
}