using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.ViewModels
{
    public class OrderedItemViewModel
    {
        public Guid ID { set; get; }
        public string Name { get; set; }

        public DateTime? NeededDate { get; set; }
        public string Description { get; set; }
        public List<string> Images { get; set; }

    }
  
}
