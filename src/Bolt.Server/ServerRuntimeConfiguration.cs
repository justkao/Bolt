namespace Bolt.Server
{
    public class ServerRuntimeConfiguration
    {
        /// <summary>
        /// Gets or sets <see cref="ISerializer"/> assigned to current context.
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// Gets or sets <see cref="ISerializer"/> assigned to current context.
        /// </summary>
        public IExceptionWrapper ExceptionWrapper { get; set; }

        /// <summary>
        /// Gets or sets <see cref="BoltServerOptions"/> assigned to current context.
        /// </summary>
        public BoltServerOptions Options { get; set; }

        /// <summary>
        /// Gets or sets <see cref="BoltServerOptions"/> assigned to current context.
        /// </summary>
        public IServerErrorHandler ErrorHandler { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IResponseHandler"/> assigned to current context.
        /// </summary>
        public IResponseHandler ResponseHandler { get; set; }

        public void Merge(ServerRuntimeConfiguration other)
        {
            if (other == null)
            {
                return;
            }

            if (other.Serializer != null)
            {
                Serializer = other.Serializer;
            }
            if (other.ExceptionWrapper != null)
            {
                ExceptionWrapper = other.ExceptionWrapper;
            }
            if (other.Options != null)
            {
                Options = other.Options;
            }
            if (other.ErrorHandler != null)
            {
                ErrorHandler = other.ErrorHandler;
            }
            if (other.Serializer != null)
            {
                Serializer = other.Serializer;
            }

            if (other.ResponseHandler != null)
            {
                ResponseHandler = other.ResponseHandler;
            }
        }
    }
}