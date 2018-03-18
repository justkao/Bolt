using System;
using System.Threading;

namespace Bolt
{
    public class ParameterMetadata
    {
        public ParameterMetadata(Type type, string name)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsCancellationToken = typeof(CancellationToken).CanAssign(Type) || typeof(CancellationToken?).CanAssign(Type);
        }

        public bool IsSerializable => !IsCancellationToken;

        public Type Type { get; }

        public string Name { get; }

        public bool IsCancellationToken { get; }
    }
}