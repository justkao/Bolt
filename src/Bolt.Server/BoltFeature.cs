using Bolt.Server.Filters;

namespace Bolt.Server
{
    public class BoltFeature : IBoltFeature
    {
        public ServerActionContext ActionContext { get; set; }

        public ISerializer Serializer { get; set; }

        public IExceptionWrapper ExceptionWrapper { get; set; }

        public BoltServerOptions Options { get; set; }

        public IServerErrorHandler ErrorHandler { get; set; }

        public IActionExecutionFilter ActionExecutionFilter { get; set; }

        public IParameterBinder ParameterBinder { get; set; }

        public IResponseHandler ResponseHandler { get; set; }
    }
}