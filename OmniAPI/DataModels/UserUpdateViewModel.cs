using SmartLifeLtd.Data.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace OmniAPI.Models
{
    public class UserUpdateViewModel
    {
        public Guid? AccountID { set; get; }
        public string Name { set; get; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ImagePath { get; set; }
    }
}
