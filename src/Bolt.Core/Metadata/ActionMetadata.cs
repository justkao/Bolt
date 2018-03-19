using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Metadata
{
    public sealed class ActionMetadata
    {
        public ActionMetadata(MethodInfo action, ParameterMetadata[] parameters, Type resultType)
        {
            Action = action;
            Parameters = new ReadOnlyCollection<ParameterMetadata>(parameters);
            ResultType = resultType;
            Timeout = TimeSpan.Zero;
            IsAsync = typeof(Task).GetTypeInfo().IsAssignableFrom(action.ReturnType.GetTypeInfo());
            NormalizedName = BoltFramework.NormalizeActionName(Name.AsReadOnlySpan()).ConvertToString();
            HasSerializableParameters = Parameters.Any(p => p.IsSerializable);
            CancellationTokenIndex = GetCancellationTokenIndex();
        }

        public string Name => Action.Name;

        public string NormalizedName { get; }

        public MethodInfo Action { get; }

        public bool IsAsync { get; }

        public IReadOnlyList<ParameterMetadata> Parameters { get; }

        public Type ResultType { get; }

        public bool HasResult => ResultType != typeof(void);

        public bool HasSerializableParameters { get; }

        public int CancellationTokenIndex { get; }

        public bool HasCancellationToken => CancellationTokenIndex >= 0;

        public bool HasParameters => Parameters.Count > 0;

        public TimeSpan Timeout { get; internal set; }

        public static void ValidateParameters(IReadOnlyList<ParameterMetadata> parameters, IReadOnlyList<object> values)
        {
            if (parameters.Count == 0)
            {
                if (values?.Count > 0)
                {
                    throw new BoltException($"No parameter values should be specified for current action.");
                }

                return;
            }

            if (values?.Count < parameters.Count)
            {
                throw new BoltException($"Not enough parameters specified.");
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                var parameterMetadata = parameters[i];
                object parameter = values[i];

                if (parameter == null)
                {
                    continue;
                }

                if (parameter is CancellationToken)
                {
                    continue;
                }

                if (!parameterMetadata.Type.GetTypeInfo().IsAssignableFrom(parameter.GetType().GetTypeInfo()))
                {
                    throw new BoltException($"Expected value for parameter '{parameterMetadata.Name}' should be '{parameterMetadata.Type.Name}' instead '{parameter.GetType().Name}' was provided.");
                }
            }
        }

        public bool IsMatch(ReadOnlySpan<char> name)
        {
            if (NormalizedName.AsReadOnlySpan().AreEqualInvariant(name))
            {
                return true;
            }

            if (NormalizedName.AsReadOnlySpan().AreEqualInvariant(BoltFramework.NormalizeActionName(name)))
            {
                return true;
            }

            return false;
        }

        public void ValidateParameters(IReadOnlyList<object> values) => ValidateParameters(Parameters, values);

        private int GetCancellationTokenIndex()
        {
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (Parameters[i].IsCancellationToken)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}