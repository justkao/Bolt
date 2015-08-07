using System;

namespace Bolt
{
    /// <summary>
    /// Marks the interface method as asynchronous.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AsyncOperationAttribute : Attribute
    {
    }
}
