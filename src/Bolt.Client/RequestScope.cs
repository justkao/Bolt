using System;
using System.Threading;

namespace Bolt.Client
{
    public class RequestScope : IDisposable
    {
        public RequestScope(CancellationToken cancellation)
            : this(TimeSpan.Zero, cancellation)
        {
        }

        public RequestScope(TimeSpan timeout)
            : this(timeout, CancellationToken.None)
        {
        }

        public RequestScope(TimeSpan timeout = default (TimeSpan), CancellationToken cancellation = default (CancellationToken))
        {
            Timeout = timeout;
            Cancellation = cancellation;
            Current = this;
        }

        public TimeSpan Timeout { get; private set; }

        public CancellationToken Cancellation { get; private set; }

        public void Dispose()
        {
            Current = null;
        }

#if !DOTNET5_4
        private const string LogicalDataKey = "__Bolt_RequestScope_Current__";

        public static RequestScope Current
        {
            get { return System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(LogicalDataKey) as RequestScope; }
            set { System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(LogicalDataKey, value); }
        }
#else
        private static readonly AsyncLocal<RequestScope> RequestScopeCurrent = new AsyncLocal<RequestScope>();

        public static RequestScope Current
        {
            get { return RequestScopeCurrent.Value; }
            set { RequestScopeCurrent.Value = value; }
        }
#endif
    }
}
