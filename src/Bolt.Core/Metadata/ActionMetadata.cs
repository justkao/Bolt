using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading;

namespace Bolt.Metadata
{
    public class ActionMetadata
    {
        internal ActionMetadata()
        {
        }

        public ActionMetadata(MethodInfo action, ParameterMetadata[] parameters, int cancellationTokenIndex, bool hasCancellationToken, bool hasSerializableParameters, Type resultType, bool hasResult)
        {
            Action = action;
            Parameters = parameters;
            CancellationTokenIndex = cancellationTokenIndex;
            HasCancellationToken = hasCancellationToken;
            HasSerializableParameters = hasSerializableParameters;
            ResultType = resultType;
            HasResult = hasResult;
        }

        public string Name => Action.Name;

        public MethodInfo Action { get; internal set; }

        public ParameterMetadata[] Parameters { get; internal set; }

        public IEnumerable<ParameterMetadata> GetSerializableParameters()
        {
            for (int i = 0; i < Parameters.Length; i++)
            {
                if (i == CancellationTokenIndex)
                {
                    continue;
                }

                yield return Parameters[i];
            }
        }

        public int CancellationTokenIndex { get; internal set; }

        public bool HasCancellationToken { get; internal set; }

        public bool HasSerializableParameters { get; internal set; }

        public bool HasParameters => Parameters.Length > 0;

        public Type ResultType { get; internal set; }

        public bool HasResult { get; internal set; }

        public void ValidateParameters(object[] parameters)
        {
            if (!Parameters.Any())
            {
                if (parameters != null && parameters.Length > 0)
                {
                    throw new BoltException($"Action '{Name}' does not require any parameters.");
                }

                return;
            }

            for (int i = 0; i < Parameters.Length; i++)
            {
                var parameterMetadata = Parameters[i];
                object parameter = parameters[i];

                if (parameter == null)
                {
                    continue;
                }

                if (parameter is CancellationToken)
                {
                    continue;
                }

                if (!parameterMetadata.Type.IsAssignableFrom(parameter.GetType()))
                {
                    throw new BoltException($"Expected value for parameter '{parameterMetadata.Name}' should be '{parameterMetadata.Type.Name}' instead '{parameter.GetType().Name}' was provided.");
                }
            }
        }
    }
}