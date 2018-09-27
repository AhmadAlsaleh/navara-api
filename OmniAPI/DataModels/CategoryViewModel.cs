using SmartLifeLtd.Data.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace OmniAPI.Models
{
    public class CategoryViewModel
    {
        public Guid? ID { set; get; }
        public string Name { set; get; }
        public string ImagePath { set; get; }
        public int? AdsNumber { get; set; }
        public CategoryViewModel Parent { set; get; } 
        public CategoryViewModel GrandParent { set; get; }
        public List<CategoryViewModel> SubCategories { set; get; }
    }
}
