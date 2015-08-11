using System;

namespace Bolt.Client
{
    public interface IErrorHandling
    {
        SessionHandlingResult Handle(ClientActionContext context, Exception e);
    }
}