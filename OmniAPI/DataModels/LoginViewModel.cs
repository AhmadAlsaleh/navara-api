using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
        //=======================
        public string Name { get; set; }
        public string CountryID { get; set; }
        public string CityID { get; set; }
        public string AreaID { get; set; }
        public string Phone { get; set; }
        public bool? IsRegister { get; set; }
    }
}
