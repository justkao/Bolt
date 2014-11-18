using System;

namespace Bolt.Generators
{
    public class IntendProvider
    {
        public virtual int Intend { get; set; }

        public virtual IDisposable Increment()
        {
            Intend++;
            return new Disposable(this, Intend - 1);
        }

        public virtual IDisposable With(int intend)
        {
            int prev = Intend;
            Intend = intend;
            return new Disposable(this, prev);
        }

        private class Disposable : IDisposable
        {
            private readonly IntendProvider _provider;
            private readonly int _oldIntend;

            public Disposable(IntendProvider provider, int oldIntend)
            {
                _provider = provider;
                _oldIntend = oldIntend;
            }

            public void Dispose()
            {
                _provider.Intend = _oldIntend;
            }
        }
    }
}
