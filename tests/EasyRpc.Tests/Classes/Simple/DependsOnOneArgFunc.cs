﻿using System;

namespace EasyRpc.Tests.Classes.Simple
{
    public interface IDependsOnOneArgFunc<T1,T2>
    {
        ITwoDependencyService<T1,T2> CreateWithT2(T2 value);
    }

    public class DependsOnOneArgFunc<T1,T2> : IDependsOnOneArgFunc<T1, T2>
    {
        private readonly Func<T2, ITwoDependencyService<T1, T2>> _func;

        public DependsOnOneArgFunc(Func<T2, ITwoDependencyService<T1, T2>> func)
        {
            _func = func;
        }

        public ITwoDependencyService<T1, T2> CreateWithT2(T2 value)
        {
            return _func(value);
        }
    }
}
