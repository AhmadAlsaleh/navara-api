using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.ViewModels
{
    public class OrderModel  
    {
        public Guid ID { set; get; }
        public List<OrderItemModel> OrderItems { get; set; }
        public string Location   { set; get; }
        public string LocationText { set; get; }
        public string LocationRemark { set; get; }
        public string FromTime { set; get; }
        public string ToTime { set; get; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Remark { get; set; }
        public string Status { set; get; }
        public string Code { set; get; }
        public DateTime? Date { get; set; }
        public double? TotalPrices { set; get; }
        public double? TotalDiscount { set; get; }
        public double? NetTotalPrices { set; get; }
        public bool? UseWallet { set; get; }
        public double? WalletAmount { set; get; }
        public int? DaysToDeliver { set; get; }
        public string InvoicePath { set; get; }
        public string PromoCode { get; set; }
    }

    public class OrderBasicModel
    {
        public Guid ID { set; get; }
        public string Name { get; set; }
        public DateTime? Date { get; set; }
        public string Code { set; get; }
        public double? TotalPrices { set; get; }
        public double? TotalDiscount { set; get; }
        public double? NetTotalPrices { set; get; }
        public string Status { set; get; }
    }

    public class OrderItemModel
    {
        public Guid? OrderItemID { get; set; }
        public Guid? OfferID { get; set; }
        public string ThumbnailImagePath { get; set; }
        public string Name { get; set; }
        public double? UnitNetPrice { get; set; }
        public double? Total { get; set; }
        public int? Quantity { set; get; }
    }
}
