namespace EasyRpc.Tests.Middleware
{
    public class GzipCompressionTests : CompressionTests
    {
        protected override string Compression => "gzip";
    }
}
