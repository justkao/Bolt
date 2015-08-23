namespace Bolt
{
    public class RuntimeActionParameters
    {
        public ActionParametersDescriptor Definition { get; set; }

        public object[] Values { get; set; }

        public void Validate()
        {
            BoltFramework.ValidateParameters(Definition.Action,Values);
        }
    }
}