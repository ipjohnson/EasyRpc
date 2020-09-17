﻿using EasyRpc.Tests.Classes.Simple;

namespace EasyRpc.Tests.Classes.Attributes
{
    [SomeTest]
    public class AttributedDependentService<T> : IDependentService<T>
    {
        public AttributedDependentService(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }

    [SomeTest(TestValue = 10)]
    public class OtherAttributedDependentService<T> : IDependentService<T>
    {
        public OtherAttributedDependentService(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
