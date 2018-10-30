using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.ViewModels
{
    public class OfferModel
    {
        public Guid ID { set; get; }
        public string Title { get; set; }
        public string ItemName { get; set; }
        public double? Price { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string OfferType { get; set; }
        public double? Discount { get; set; }
        public bool? IsActive { set; get; }
        public string ThumbnailImagePath { set; get; }
        public Guid? ItemID { get; set; }
    }
    public class OfferFullModel
    {
        public Guid ID { set; get; }
        public string Title { get; set; }
        public string ItemName { get; set; }
        public double? Price { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string OfferType { get; set; }
        public double? Discount { get; set; }
        public double? UnitNetPrice { get; set; }
        public double? UnitPrice { get; set; }
        public bool? IsActive { set; get; }
        public string ThumbnailImagePath { set; get; }
        public Guid? ItemID { get; set; }
        public List<string> OfferImages { set; get; }
        public List<ItemBasicModel> OfferItems { set; get; }
    }
    public class OfferBasicModel
    {
        public Guid ID { set; get; }
        public string Title { get; set; }
        public string ItemName { get; set; }
        public string ShortDescription { get; set; }
        public string OfferType { get; set; }
        public string ThumbnailImagePath { get; set; }
        public double? Discount { get; set; }
        public double? UnitNetPrice { get; set; }
        public double? UnitPrice { get; set; }
    }
}
