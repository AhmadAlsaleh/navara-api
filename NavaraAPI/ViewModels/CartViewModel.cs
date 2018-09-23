using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; }
        public Guid? AccountID { set; get; }
        public DateTime? CreatedDate { set; get; }
    }
    public class CartItemViewModel
    {
        public Guid? ItemID { set; get; }
        public int? Quantity { set; get; }
        public double? UnitPrice { set; get; }
        public double? UnitDiscount { set; get; }
        public double? UnitNetPrice { set; get; }
        public double? Total { set; get; }
        public bool? IsFree { set; get; }
        public Guid? OfferID { set; get; }
    }

    public class CartOfferViewModel
    {
        public Guid? OfferID { set; get; }
        public int? Quantity { set; get; }
    }
}
