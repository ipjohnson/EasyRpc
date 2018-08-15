namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Documentation configuration
    /// </summary>
    public class DocumentationConfiguration
    {
        /// <summary>
        /// Menu width in rem
        /// </summary>
        public double? MenuWidth { get; set; }

        /// <summary>
        /// Title for documentation
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Custom Css url
        /// </summary>
        public string CustomCss { get; set; }

        /// <summary>
        /// Custom url for all documentation resources.
        /// </summary>
        public string CustomBaseUrl { get; set; }
    }
}
