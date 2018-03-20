using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client.Helpers
{
    internal sealed class AsyncLock : IDisposable
    {
        private readonly Releaser _releaser;
        private readonly Task<IDisposable> _releaserAsync;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public AsyncLock()
        {
            _releaser = new Releaser(this);
            _releaserAsync = Task.FromResult((IDisposable)_releaser);
        }

        public IDisposable Lock(CancellationToken cancellation = default(CancellationToken))
        {
            _semaphore.Wait(cancellation);

            return _releaser;
        }

        public Task<IDisposable> LockAsync(CancellationToken cancellation = default(CancellationToken))
        {
            Task wait = _semaphore.WaitAsync(cancellation);
            if (wait.IsCanceled)
            {
                cancellation.ThrowIfCancellationRequested();
            }

            if (wait.IsCompleted)
            {
                return _releaserAsync;
            }

            return wait.ContinueWith(
                (t, state) =>
                {
                    if (t.IsCanceled)
                    {
                        t.GetAwaiter().GetResult();
                    }

                    return (IDisposable)state;
                },
                _releaser,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
            _semaphore = null;
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock _release;

            internal Releaser(AsyncLock toRelease)
            {
                _release = toRelease;
            }

            public void Dispose()
            {
                _release._semaphore.Release();
            }
        }
    }
}
