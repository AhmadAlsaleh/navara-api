using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class RegisterViewModel
    {
        [Required]

        [Display(Name = "User Name")]
        [DataType(DataType.Text, ErrorMessage = " name should not contain Empty spaces and  not exist befor")]
        public string Name { get; set; }
        public bool RememberMe { get; set; }
        public string CountryID { get; set; }
        public string CityID { get; set; }
        [Required(ErrorMessage = "you must select Location")]
        public string AreaID { get; set; }
        public string Phone { get; set; }


        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        //[DataType(DataType.Password,ErrorMessage = "Password must contain uppercase letters and characters such as {A,,a,,_,,-,,*")]
        [Display(Name = "Password")]
        public string Password { get; set; }

         


    }
    public class RegisterAPIViewModel
    {
        [Required]

        [Display(Name = "User Name")]
        [DataType(DataType.Text, ErrorMessage = " name should not contain Empty spaces and  not exist befor")]
        public string Name { get; set; }
        public bool RememberMe { get; set; }
        public string CountryID { get; set; }
        public string CityID { get; set; }
        public string AreaID { get; set; }
        public string Phone { get; set; }


        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        //[DataType(DataType.Password,ErrorMessage = "Password must contain uppercase letters and characters such as {A,,a,,_,,-,,*")]
        [Display(Name = "Password")]
        public string Password { get; set; }



    }
}
