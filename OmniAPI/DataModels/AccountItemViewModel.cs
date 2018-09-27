using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class AccountItemViewModel
    {
        public Guid? ID { set; get; }
        public string Code { set; get; }
        public string Name { set; get; }
        public string Title { set; get; }
        public string CurrencyName { set; get; }
        public DateTime? PublishedDate { get; set; }
        public int? ViewsNum { set; get; }
        public bool? IsNegotiable { get; set; }
        public int? FavNum { get; set; }
        public string MainImage { set; get; }
        public double? Price { set; get; }
        public bool? IsFeatured { set; get; }
    }
}
