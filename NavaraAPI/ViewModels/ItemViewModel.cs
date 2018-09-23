using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.ViewModels
{
    public class ItemModel
    {
        public Guid ID { set; get; }
        public string Name { get; set; }
        public int? Quantity { get; set; }
        public double? Price { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public bool? IsEnable { set; get; }
        public string ThumbnailImagePath { set; get; }
        public Guid? ItemCategoryID { get; set; }
    }
    public class ItemFullModel
    {
        public Guid ID { set; get; }
        public string Name { get; set; }
        public int? Quantity { get; set; }
        public double? Price { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public bool? IsEnable { set; get; }
        public string ThumbnailImagePath { set; get; }
        public string ItemCategory { set; get; }
        public Guid? ItemCategoryID { get; set; }
        public List<string> ItemImages { set; get; }
    }
    public class ItemBasicModel
    {
        public Guid ID { set; get; }
        public string Name { get; set; }
        public double? Price { set; get; }
        public int? Quantity { set; get; }
        public string ItemCategory { set; get; }
        public Guid? ItemCategoryID { get; set; }
        public string ShortDescription { get; set; }
        public string ThumbnailImagePath { get; set; }
    }
}
