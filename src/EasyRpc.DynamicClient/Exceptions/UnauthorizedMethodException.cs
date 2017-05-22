namespace EasyRpc.DynamicClient.Exceptions
{
    public class UnauthorizedMethodException : DynamicMethodException
    {
        public UnauthorizedMethodException(string method) : base(method, "Unauthorized access to this method")
        {

        }
    }
}
