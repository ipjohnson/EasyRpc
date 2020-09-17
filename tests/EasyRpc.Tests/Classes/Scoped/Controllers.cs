﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EasyRpc.Tests.Classes.Scoped
{
    public class TestController1 : IDisposable
    {
        private static int counter;

        private static int disposeCount;

        public TestController1(
            IRepositoryTransient1 transient1,
            IRepositoryTransient2 repositoryTransient2,
            IRepositoryTransient3 repositoryTransient3,
            IRepositoryTransient4 repositoryTransient4,
            IRepositoryTransient5 repositoryTransient5)
        {
            if (transient1 == null)
            {
                throw new ArgumentNullException(nameof(transient1));
            }

            if (repositoryTransient2 == null)
            {
                throw new ArgumentNullException(nameof(repositoryTransient2));
            }

            if (repositoryTransient3 == null)
            {
                throw new ArgumentNullException(nameof(repositoryTransient3));
            }

            if (repositoryTransient4 == null)
            {
                throw new ArgumentNullException(nameof(repositoryTransient4));
            }

            if (repositoryTransient5 == null)
            {
                throw new ArgumentNullException(nameof(repositoryTransient5));
            }

            Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }

        public static int DisposeCount
        {
            get { return disposeCount; }
            set { disposeCount = value; }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Interlocked.Increment(ref disposeCount);
        }
    }
}
