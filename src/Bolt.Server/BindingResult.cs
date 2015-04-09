namespace Bolt.Server
{
    public class BindingResult
    {
        public static readonly BindingResult Empty = new BindingResult();

        private BindingResult()
        {
            Success = false;
        }

        public BindingResult(object parameters)
        {
            Parameters = parameters;
            Success = true;
        }

        public object Parameters { get; private set; }

        public bool Success { get; private set; }
    }
}