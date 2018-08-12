namespace EasyRpc.TestApp.Models
{
    /// <summary>
    /// Represents a mailing address
    /// </summary>
    public class AddressModel
    {
        /// <summary>
        /// Line one
        /// </summary>
        public string Line1 { get; set; }

        /// <summary>
        /// Line two
        /// </summary>
        public string Line2 { get; set; }

        /// <summary>
        /// City
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// State
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Postal Code
        /// </summary>
        public string PostalCode { get; set; }
    }
}
