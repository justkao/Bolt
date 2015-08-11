using System;

namespace Bolt.Pipeline
{
    public interface IPipeline<in T> : IDisposable where T : ActionContextBase
    {
        ActionDelegate<T> Instance { get; }
    }
}