using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class Contact
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Name { get; set; }

        [Required]
        public string Subject { get; set; }
        //[Required]
        //public string Name { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
