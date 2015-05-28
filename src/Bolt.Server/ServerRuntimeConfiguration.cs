using Bolt.Server.InstanceProviders;
using System;

namespace Bolt.Server
{
    public class ServerRuntimeConfiguration
    {
        public ServerRuntimeConfiguration()
        {
        }

        public ServerRuntimeConfiguration(ServerRuntimeConfiguration other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Serializer = other.Serializer;
            ExceptionWrapper = other.ExceptionWrapper;
            Options = other.Options;
            ErrorHandler = other.ErrorHandler;
            ResponseHandler = other.ResponseHandler;
            ParameterHandler = other.ParameterHandler;
        }

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

        /// <summary>
        /// Gets or sets <see cref="IParameterHandler"/> assigned to current context.
        /// </summary>
        public IParameterHandler ParameterHandler { get; set; }

        /// <summary>
        /// Gets or sets <see cref="ISessionFactory"/> assigned to current context.
        /// </summary>
        public ISessionFactory SessionFactory { get; set; }

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
            if (other.ParameterHandler != null)
            {
                ParameterHandler = other.ParameterHandler;
            }
            if (other.SessionFactory != null)
            {
                SessionFactory = other.SessionFactory;
            }
        }
    }
}