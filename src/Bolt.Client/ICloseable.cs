using System;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public interface ICloseable : IDisposable
    {
        bool IsClosed { get; }

        void Close();

        Task CloseAsync();
    }
}
