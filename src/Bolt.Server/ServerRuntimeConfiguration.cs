using System;
using System.Collections.Generic;
using Bolt.Serialization;

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

            AvailableSerializers = other.AvailableSerializers;
            DefaultSerializer = other.DefaultSerializer;
            ExceptionSerializer = other.ExceptionSerializer;
            Options = other.Options;
            ErrorHandler = other.ErrorHandler;
        }

        public ISerializer DefaultSerializer { get; set; }

        public IReadOnlyList<ISerializer> AvailableSerializers { get; set; }

        public IExceptionSerializer ExceptionSerializer { get; set; }

        public BoltServerOptions Options { get; set; }

        public IServerErrorHandler ErrorHandler { get; set; }

        public void Reset()
        {
            DefaultSerializer = null;
            AvailableSerializers = null;
            ExceptionSerializer = null;
            Options = null;
            ErrorHandler = null;
        }

        public void Merge(ServerRuntimeConfiguration other)
        {
            if (other == null)
            {
                return;
            }

            if (other.AvailableSerializers != null)
            {
                AvailableSerializers = other.AvailableSerializers;
            }
            if (other.ExceptionSerializer != null)
            {
                ExceptionSerializer = other.ExceptionSerializer;
            }
            if (other.Options != null)
            {
                Options = other.Options;
            }
            if (other.DefaultSerializer != null)
            {
                DefaultSerializer = other.DefaultSerializer;
            }
            if (other.ErrorHandler != null)
            {
                ErrorHandler = other.ErrorHandler;
            }
        }
    }
}