using SmartLifeLtd.Data.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace OmniAPI.Models
{
    public class CategoryFullDataModel
    {
        public Guid? ID { set; get; }
        public string Name { set; get; }
        public string ImagePath { set; get; }
        public int? AdsNumber { get; set; }
        public CategoryDataModel Parent { set; get; }
        public CategoryDataModel GrandParent { set; get; }
        public List<CategoryDataModel> SubCategories { set; get; }
        public List<string> Options { set; get; }
        public Guid? ParentID { set; get; }
        public string Color { set; get; }
        public bool? HasChildren { set; get; }
        public List<CategoryFieldDataModel> categoryFields { set; get; }
    }

    public class CategoryDataModel
    {
        public Guid? ID { set; get; }
        public string Name { set; get; }
        public string ImagePath { set; get; }
        public int? AdsNumber { get; set; }
        public CategoryDataModel Parent { set; get; } 
        public CategoryDataModel GrandParent { set; get; }
        public List<CategoryDataModel> SubCategories { set; get; }
    }
}
