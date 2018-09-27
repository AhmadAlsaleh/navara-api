using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class ReportViewModel
    {
        [Required]
        public Guid? UserID { get; set; }
        [Required]
        public Guid AddID { get; set; }
        public string Body { get; set; }
    }
}
