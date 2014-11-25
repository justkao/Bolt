namespace TestService.Core
{
    public class ParametersBase
    {
        public ParametersBase()
            : this(10)
        {
        }

        public ParametersBase(string someeeeeee)
        {
            Someeeeeee = someeeeeee;
        }

        protected ParametersBase(int someeeeeee)
        {
        }

        public string Someeeeeee { get; set; }
    }
}