using System;

namespace Bolt
{
    /// <summary>
    /// Indicating the action that should close the session.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CloseSessionAttribute : Attribute
    {
    }
}