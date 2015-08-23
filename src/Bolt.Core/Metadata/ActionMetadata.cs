using System;
using System.Reflection;

namespace Bolt.Metadata
{
    public class ActionMetadata
    {
        internal ActionMetadata()
        {
        }

        public MethodInfo Action { get; internal set; }

        public ParameterDescriptor[] Parameters { get; internal set; }

        public int CancellationTokenIndex { get; internal set; }

        public bool HasCancellationToken { get; internal set; }

        public bool HasSerializableParameters { get; internal set; }

        public Type ResultType { get; internal set; }
        public bool HasResult { get; internal set; }
    }
}