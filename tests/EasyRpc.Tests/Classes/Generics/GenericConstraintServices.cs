using EasyRpc.Tests.Classes.Simple;

namespace EasyRpc.Tests.Classes.Generics
{
    public interface IGenericConstraintService<T>
    {
        T Value { get; }
    }

    public class GenericConstraintServiceA<T> : IGenericConstraintService<T>
    {
        public T Value { get; }
    }


    public class GenericConstraintBasicService<T> : IGenericConstraintService<T> where T : IBasicService 
    {
        public T Value { get; }
    }
}
