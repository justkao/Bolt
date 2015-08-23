using System;
using System.Reflection;

namespace Bolt
{
    public class ActionParametersDescriptor
    {
        internal ActionParametersDescriptor()
        {
        }

        public ActionParametersDescriptor(MethodInfo action, ParameterDescriptor[] parameters, int cancellationTokenIndex, bool hasCancellationToken, bool hasSerializableParameters)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            Action = action;
            Parameters = parameters;
            CancellationTokenIndex = cancellationTokenIndex;
            HasCancellationToken = hasCancellationToken;
            HasSerializableParameters = hasSerializableParameters;
        }

        public MethodInfo Action { get; internal set; }

        public ParameterDescriptor[] Parameters { get; internal set; }

        public int CancellationTokenIndex { get; internal set; }

        public bool HasCancellationToken { get; internal set; }

        public bool HasSerializableParameters { get; internal set; }
    }
}