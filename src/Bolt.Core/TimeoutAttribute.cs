using System;

namespace Bolt
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TimeoutAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets timeout in miliseconds.
        /// </summary>
        public int Timeout { get; set; }
    }
}