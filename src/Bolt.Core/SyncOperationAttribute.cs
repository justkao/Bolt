using System;

namespace Bolt
{
    /// <summary>
    /// Marks the interface method as synchronous.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SyncOperationAttribute : Attribute
    {
    }
}