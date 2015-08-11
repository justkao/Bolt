using System;

namespace Bolt.Client
{
    public interface IErrorHandling
    {
        ErrorHandlingResult Handle(ClientActionContext context, Exception e);
    }
}