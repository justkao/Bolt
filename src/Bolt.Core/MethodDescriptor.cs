using System;

namespace Bolt
{
    public class MethodDescriptor
    {
        public MethodDescriptor(string contract, string method, string url, Type parameters)
        {
            Contract = contract;
            Method = method;
            Url = url;
            Parameters = parameters;
        }

        public string Contract { get; private set; }

        public string Method { get; private set; }

        public string Url { get; private set; }

        public Type Parameters { get; set; }
    }
}