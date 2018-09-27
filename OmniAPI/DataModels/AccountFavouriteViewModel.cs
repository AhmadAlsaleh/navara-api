using SmartLifeLtd.Data.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace OmniAPI.Models
{
    public class AccountFavouriteViewModel
    {
        public Guid? ID { set; get; }
        public string Code { set; get; }
        public string Name { set; get; }
        public string Title { set; get; }
        public string Currency { set; get; }
        public int? ViewsNum { set; get; }
        public int? FavNum { get; set; }
        public string Image { set; get; }
        public double? Price { set; get; }
        public bool? IsFeatured { set; get; }
        public bool? IsNegotiable { get; set; }
    }
}
