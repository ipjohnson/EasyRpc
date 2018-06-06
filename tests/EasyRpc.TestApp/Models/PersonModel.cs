using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.TestApp.Models
{
    /// <summary>
    /// Model represents a person
    /// </summary>
    public class PersonModel
    {
        /// <summary>
        /// Id for person model
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// First Name
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// Last Name
        /// </summary>
        [Required]
        public string LastName { get; set; }

        /// <summary>
        /// Age
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        public string EmailAddress { get; set; }
        
        /// <summary>
        /// Home Address
        /// </summary>
        public AddressModel HomeAddress { get; set; }

        /// <summary>
        /// Work address
        /// </summary>
        public AddressModel WorkAddress { get; set; }
    }
}
