namespace EasyRpc.DynamicClient.Exceptions
{
    public class InternalServerErrorException : DynamicMethodException
    {
        public InternalServerErrorException(string method, string message) : base(method, message)
        {
        }
    }
}
