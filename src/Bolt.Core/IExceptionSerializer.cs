using System;

namespace Bolt
{
    public interface IExceptionSerializer
    {
        string Serialize(Exception exception);

        Exception Deserialize(string exception);
    }
}
