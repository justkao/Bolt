using Bolt.Server.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Xunit;

namespace Bolt.Server.Test
{
    public class SessionHandlerTest
    {
        public SessionHandlerTest()
        {
            Options = new BoltServerOptions { SessionHeader = "Test" };
            Subject = new ServerSessionHandler(Options);
        }

        public BoltServerOptions Options { get; set; }

        public IServerSessionHandler Subject { get; set; }

        [Fact]
        public void Initialize_NotNull()
        {
            Assert.NotNull(Subject.Initialize(new DefaultHttpContext()));
        }

        [Fact]
        public void Initialize_SessionHeaderCreated()
        {
            var ctxt = new DefaultHttpContext();
            var session = Subject.Initialize(ctxt);

            Assert.Equal(session, ctxt.Response.Headers[Options.SessionHeader]);
        }

        [Fact]
        public void GetIdentifier_ExistingSession_NotNull()
        {
            var ctxt = new DefaultHttpContext();
            var session = Subject.Initialize(ctxt);

            Assert.Equal(session, Subject.GetIdentifier(ctxt));
        }

        [Fact]
        public void GetIdentifier_NoSession_Null()
        {
            var ctxt = new DefaultHttpContext();

            Assert.Null(Subject.GetIdentifier(ctxt));
        }
    }
}
