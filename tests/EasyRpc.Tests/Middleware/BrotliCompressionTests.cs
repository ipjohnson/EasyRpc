namespace EasyRpc.Tests.Middleware
{
    public class BrotliCompressionTests : CompressionTests
    {
        protected override string Compression => "br";
    }
}
