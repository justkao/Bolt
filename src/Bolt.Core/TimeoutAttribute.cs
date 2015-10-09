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
        /// Gets or sets timeout in miliseconds.
        /// </summary>
        public int Timeout { get; set; }
    }
}