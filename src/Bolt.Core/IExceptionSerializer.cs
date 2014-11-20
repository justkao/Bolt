using System;

namespace Bolt
{
    public interface IExceptionSerializer
    {
        byte[] Serialize(Exception exception);

        Exception Deserialize(byte[] exception);
    }
}
