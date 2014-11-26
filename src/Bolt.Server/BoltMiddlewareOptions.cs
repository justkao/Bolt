using System;

namespace Bolt.Server
{
    public class BoltMiddlewareOptions
    {
        public BoltMiddlewareOptions(BoltContainer boltContainer)
        {
            if (boltContainer == null)
            {
                throw new ArgumentNullException("boltContainer");
            }

            BoltContainer = boltContainer;
        }

        public BoltContainer BoltContainer { get; private set; }
    }
}