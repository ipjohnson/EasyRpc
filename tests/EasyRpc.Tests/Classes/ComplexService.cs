namespace EasyRpc.Tests.Classes
{
    public interface IComplexService
    {
        ResultObject Add(ComplexObject complex);
    }

    public class ComplexObject
    {
        public int A { get; set; }

        public int B { get; set; }
    }

    public class ResultObject
    {
        public int Result { get; set; }
    }


    public class ComplexService : IComplexService
    {
        public ResultObject Add(ComplexObject complex)
        {
            return new ResultObject { Result = complex.A + complex.B };
        }
    }
}
