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
        public int? HighPrice { get; set; }
        public int? LowPrice { get; set; }
    }

    public class SavedSearchDataModel
    {
        public string Name { get; set; }
        public DateTime? SearchDate { set; get; }
        public int? FromPrice { set; get; }
        public int? ToPrice { get; set; }
        public bool? SearchInDescription { get; set; }
        public Guid? CategoryID { get; set; }
        public string Keywords { get; set; }

    }
}
