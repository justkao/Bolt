using System;

namespace Bolt
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TimeoutAttribute : Attribute
    {
        public TimeoutAttribute()
        {
        }

        public TimeoutAttribute(int timeout)
        {
            Timeout = timeout;
        }

        /// <summary>
        /// Gets or sets the timeout in milliseconds.
        /// </summary>
        public int Timeout { get; set; }
    }
}