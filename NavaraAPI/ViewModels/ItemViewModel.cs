using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.ViewModels
{
    public class ItemViewModel
    {
        public string Name { get; set; }
        public double? Price { set; get; }
        public int? Quantity { set; get; }
        public Guid? ItemCategoryID { set; get; }
        public string Description  { get; set; }
    }
}
