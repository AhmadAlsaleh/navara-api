using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class AddToFavoriteViewModel
    {
        public Guid UserID { get; set; }
        public Guid AdID { get; set; }
    }
}
