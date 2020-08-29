using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EasyRpc.AspNetCore.Data
{
    /// <summary>
    /// Lock-less object pool
    /// </summary>
    public class ObjectPool<T>
    {
        private readonly Func<T> _createFunc;

        private int _max;
        private int _count;
        private PoolObjectInstance _pool;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="createFunc"></param>
        /// <param name="max"></param>
        public ObjectPool(Func<T> createFunc, int max = 0)
        {
            _max = max;

            _createFunc = createFunc;
        }

        /// <summary>
        /// Total instances 
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// A
        /// </summary>
        /// <returns></returns>
        public IPoolObjectInstance AcquireInstance()
        {
            var current = _pool;

            if (current != null)
            {
                if (Interlocked.CompareExchange(ref _pool, current.Next, current) == current)
                {
                    return current;
                }
            }

            if (_max == 0)
            {
                return CreateNewInstance();
            }
            
            if (_count < _max)
            {
                var newCount = Interlocked.Add(ref _count, 1);

                if (newCount <= _max)
                {
                    return CreateNewInstance();
                }
                
                Interlocked.Decrement(ref _count);
            }

            Thread.Sleep(1);

            return AcquireInstance();
        }

        private IPoolObjectInstance CreateNewInstance()
        {
            return new PoolObjectInstance(this, _createFunc());
        }

        #region PoolObjectInstance
        /// <summary>
        /// 
        /// </summary>
        public interface IPoolObjectInstance : IDisposable
        {
            /// <summary>
            /// Pool object instance
            /// </summary>
            T Instance { get; }
        }

        private void ReturnInstance(PoolObjectInstance instance)
        {
            var current = _pool;

            instance.Next = current;

            if (Interlocked.CompareExchange(ref _pool, instance, current) != current)
            {
                SpinWaitReturn(instance);
            }
        }

        private void SpinWaitReturn(PoolObjectInstance instance)
        {
            var spin = new SpinWait();

            PoolObjectInstance current;

            do
            {
                spin.SpinOnce();

                 current = _pool;

                instance.Next = current;

            } while (Interlocked.CompareExchange(ref _pool, instance, current) != current);
        }

        private class PoolObjectInstance : IPoolObjectInstance
        {
            private ObjectPool<T> _pool;

            public PoolObjectInstance(ObjectPool<T> pool, T instance)
            {
                _pool = pool;
                Instance = instance;
            }
            
            /// <inheritdoc />
            public void Dispose()
            {
                _pool.ReturnInstance(this);
            }

            /// <inheritdoc />
            public T Instance { get; }

            public PoolObjectInstance Next { get; set; }
        }
        #endregion
    }
}
