using System;

namespace Bolt.Generators
{
    internal class EasyDispose : IDisposable
    {
        private readonly Action _disposeAction;

        public EasyDispose(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction();
        }
    }
}
