using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Core;

namespace Bolt.Client.Pipeline
{
    public class WriteParametersHandler : IClientContextHandler
    {
        public WriteParametersHandler(ISerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            Serializer = serializer;
        }

        public HandleContextStage Stage => HandleContextStage.Before;

        public ISerializer Serializer { get; }

        public virtual Task HandleAsync(ClientActionContext context, Func<ClientActionContext, Task> next)
        {
            if (context.Request.Content == null && context.HasSerializableParameters)
            {
                context.Request.Content = BuildRequestContent(context);
            }

            return next(context);
        }

        private HttpContent BuildRequestContent(ClientActionContext context)
        {
            IObjectSerializer parameterSerializer = Serializer.CreateSerializer();
            ParameterInfo[] parameters = context.Action.GetParameters();

            for (int i = 0; i < context.Parameters.Length; i++)
            {
                if (context.Parameters[i] == null)
                {
                    continue;
                }

                if (context.Parameters[i] is CancellationToken)
                {
                    continue;
                }

                parameterSerializer.WriteParameter(context.Action, parameters[i].Name, parameters[i].GetType(), parameters[i]);
            }

            if (parameterSerializer.IsEmpty)
            {
                return null;
            }

            Stream resultStream = parameterSerializer.GetOutputStream();
            byte[] rawData = null;
            if (resultStream is MemoryStream)
            {
                rawData = (resultStream as MemoryStream).ToArray();
            }
            else
            {
                rawData = resultStream.Copy().ToArray();
            }

            ByteArrayContent content = new ByteArrayContent(rawData);
            content.Headers.ContentLength = rawData.Length;
            context.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Serializer.ContentType));
            return content;
        }
    }
}
