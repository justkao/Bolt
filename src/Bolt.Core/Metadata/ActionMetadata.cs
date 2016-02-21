using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;
using System.Threading;

namespace Bolt.Metadata
{
    public sealed class ActionMetadata
    {
        private ReadOnlyCollection<ParameterMetadata> _serializableParameters;
        private int? _cancellationTokenIndex;

        internal ActionMetadata()
        {
            Timeout = TimeSpan.Zero;
        }

        public ActionMetadata(MethodInfo action, ParameterMetadata[] parameters, Type resultType)
        {
            Action = action;
            Parameters = new ReadOnlyCollection<ParameterMetadata>(parameters);
            ResultType = resultType;
            Timeout = TimeSpan.Zero;
        }

        public string Name => Action.Name;

        public MethodInfo Action { get; internal set; }

        public IReadOnlyList<ParameterMetadata> Parameters { get; internal set; }

        public IReadOnlyList<ParameterMetadata> SerializableParameters
        {
            get
            {
                if (_serializableParameters == null)
                {
                    _serializableParameters = new ReadOnlyCollection<ParameterMetadata>(GetSerializableParameters().ToArray());
                }

                return _serializableParameters;
            }
        }

        public int CancellationTokenIndex
        {
            get
            {
                if (_cancellationTokenIndex.HasValue)
                {
                    return _cancellationTokenIndex.Value;
                }

                int index = -1;
                for (int i = 0; i < Parameters.Count; i++)
                {
                    if (typeof(CancellationToken).CanAssign(Parameters[i].Type) || typeof(CancellationToken?).CanAssign(Parameters[i].Type))
                    {
                        index = i;
                    }
                }

                _cancellationTokenIndex = index;
                return index;
            }
        }

        public Type ResultType { get; internal set; }

        public bool HasResult => ResultType != typeof(void);

        public bool HasSerializableParameters => SerializableParameters.Count > 0;

        public bool HasCancellationToken => CancellationTokenIndex >= 0;

        public bool HasParameters => Parameters.Count > 0;

        public TimeSpan Timeout { get; internal set; }

        public void ValidateParameters(object[] parameters)
        {
            if (Parameters.Count == 0)
            {
                if (parameters != null && parameters.Length > 0)
                {
                    throw new BoltException($"Action '{Name}' does not require any parameters.");
                }

                return;
            }

            for (int i = 0; i < Parameters.Count; i++)
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

        private IEnumerable<ParameterMetadata> GetSerializableParameters()
        {
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (i == CancellationTokenIndex)
                {
                    continue;
                }

                yield return Parameters[i];
            }
        }

    }
}