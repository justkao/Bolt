using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client.Helpers
{
    internal class AwaitableCriticalSection
    {
        private class Token : IAsyncResult
        {
            private readonly ManualResetEvent _event = new ManualResetEvent(false);
            private bool _synchronous;

            public object AsyncState => null;

            public WaitHandle AsyncWaitHandle => _event;

            public bool CompletedSynchronously => _synchronous;

            public bool IsCompleted => _event.WaitOne(TimeSpan.Zero);

            public void Signal(bool synchronous)
            {
                _synchronous = synchronous;
                _event.Set();
            }
        }

        private class Disposable : IDisposable
        {
            private readonly AwaitableCriticalSection _owner;

            public Disposable(AwaitableCriticalSection owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                _owner.Exit();
            }
        }

        private readonly Queue<Token> _tokens = new Queue<Token>();
        private bool _busy;
        private readonly object _syncRoot = new object();
        private readonly IDisposable _disposable;

        public AwaitableCriticalSection()
        {
            _disposable = new Disposable(this);
        }

        public Task<IDisposable> EnterAsync()
        {
            Token token = new Token();
            lock (_syncRoot)
            {
                _tokens.Enqueue(token);
                if (!_busy)
                {
                    _busy = true;
                    _tokens.Dequeue().Signal(true);
                }
            }

            return Task.Factory.FromAsync(token, result => _disposable);
        }

        public IDisposable Enter()
        {
            Token token = new Token();
            lock (_syncRoot)
            {
                _tokens.Enqueue(token);
                if (!_busy)
                {
                    _busy = true;
                    _tokens.Dequeue().Signal(true);
                }
            }

            token.AsyncWaitHandle.WaitOne();
            return _disposable;
        }

        private void Exit()
        {
            lock (_syncRoot)
            {
                if (_tokens.Any())
                {
                    _tokens.Dequeue().Signal(false);
                }
                else
                {
                    _busy = false;
                }
            }
        }
    }
}
