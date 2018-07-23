namespace EasyRpc.AspNetCore
{
    public class RpcServiceConfiguration
    {
        /// <summary>
        /// Show error message back to client
        /// </summary>
        public bool ShowErrorMessage { get; set; } = true;

        /// <summary>
        /// Turn on debug logging
        /// </summary>
        public bool DebugLogging { get; set; } = false;

        /// <summary>
        /// Support Content-Encoding: gzip for response
        /// </summary>
        public bool SupportResponseCompression { get; set; } = false;

        /// <summary>
        /// Support Content-Encoding: gzip for request
        /// </summary>
        public bool SupportRequestCompression { get; set; } = false;

        /// <summary>
        /// Interface parameters that are not IEnumerable will be automatically resolved from request services
        /// </summary>
        public bool AutoResolveInterfaces { get; set; } = true;
    }
}
