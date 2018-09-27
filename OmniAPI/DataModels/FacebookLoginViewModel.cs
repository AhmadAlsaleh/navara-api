using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class FacebookLoginViewModel
    {
        public string ImagePath { get; set; }
        public string FacebookID { get; set; }
        public string Name { get; set; }
        public bool RememberMe { get; set; }
        public string CountryID { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
