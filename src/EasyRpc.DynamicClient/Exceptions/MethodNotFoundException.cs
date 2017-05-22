namespace EasyRpc.DynamicClient.Exceptions
{
    public class MethodNotFoundException : DynamicMethodException
    {
        public MethodNotFoundException(string method) : base(method, $"Could not find method for {method}")
        {

        }
    }
}
