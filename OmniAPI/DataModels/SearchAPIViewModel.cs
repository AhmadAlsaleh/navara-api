using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class SearchAPIViewModel
    {
        public Guid UserID { get; set; }
        public string SearchWord { get; set; }
    }
}
