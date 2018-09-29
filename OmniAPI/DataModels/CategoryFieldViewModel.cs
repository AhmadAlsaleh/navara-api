using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class CategoryFieldDataModel
    {
        public Guid? CategoryFieldID { get; set; }
        public string CategoryFieldName { get; set; }
        public string CategoryFieldType { get; set; }
        public List<CategoryFieldOptionDataModel> CategoryFieldOption { set; get; }
    }

    public class CategoryFieldOptionDataModel
    {
        public Guid? CategoryFieldID { get; set; }
        public Guid? CategoryFieldOptionID { get; set; }
        public string Value { get; set; }
    }
}
