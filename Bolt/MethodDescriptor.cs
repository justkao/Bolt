namespace Bolt
{
    public class MethodDescriptor
    {
        public MethodDescriptor()
        {
        }

        public MethodDescriptor(string contract, string method, string url)
        {
            Contract = contract;
            Method = method;
            Url = url;
        }

        public string Contract { get; private set; }

        public string Method { get; private set; }

        public string Url { get; private set; }
    }
}