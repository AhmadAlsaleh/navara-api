using SmartLifeLtd.Data.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace OmniAPI.Models
{
    public class SearchDataModel
    {
        public int Page { get; set; } = 0;
        public int Count { set; get; } = 10;
        public string SortingType { set; get; }
        public Guid? CategoryID { get; set; }
        public string SearchWord { get; set; }
        public double? HighPrice { get; set; }
        public double? LowPrice { get; set; }
        public Guid? AreaID { get; set; }

    }
}
