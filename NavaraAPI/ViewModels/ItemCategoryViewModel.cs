using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.ViewModels
{
    public class ItemCategoryModel
    {
        public Guid ID { set; get; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Description  { get; set; }
        public string ImagePath { get; set; }
    }
}
