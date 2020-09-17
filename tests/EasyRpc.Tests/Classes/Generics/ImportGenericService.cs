﻿namespace EasyRpc.Tests.Classes.Generics
{
    public interface IImportGenericService<T>
    {
        T Value { get; }
    }

    public class ImportGenericService<T> : IImportGenericService<T>
    {
        public ImportGenericService(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }

    public class SecondImportGenericService<T> : IImportGenericService<T>
    {
        public SecondImportGenericService(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
