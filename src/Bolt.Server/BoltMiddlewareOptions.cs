using System;

namespace Bolt.Server
{
    public class BoltMiddlewareOptions
    {
        public BoltMiddlewareOptions(IBoltExecutor boltExecutor)
        {
            if (boltExecutor == null)
            {
                throw new ArgumentNullException("boltExecutor");
            }

            BoltExecutor = boltExecutor;
        }

        public IBoltExecutor BoltExecutor { get; private set; }
    }
}