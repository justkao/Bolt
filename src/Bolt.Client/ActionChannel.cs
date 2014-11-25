using System.Threading;

namespace Bolt.Client
{
    public class ActionChannel : ChannelBase
    {
        private readonly ClientActionContext _context;

        public ActionChannel(ActionChannel channel)
            : base(channel)
        {
            _context = channel._context;
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