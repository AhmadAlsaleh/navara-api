using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class UpdatePasswordViewModel
    {
        public Guid? AccountID { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

    }
}
