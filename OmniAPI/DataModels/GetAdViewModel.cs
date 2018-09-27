using SmartLifeLtd.Data.Tables.Omni;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class GetAdViewModel
    {
        public Guid? ID { get; set; }
        public Guid? CategoryID { get; set; }
        public string CityName { get; set; }
        public Category Category  { get; set; }
        public string CurrencyName { get; set; }
        public string Name { get; set; }
        public int? Hearts { get; set; }
        public DateTime PublishedDate { get; set; }
        public int? View { get; set; }
        public double? Price { get; set; }
        public string Title { get; set; }
        public bool? IsDisabled { get; set; }
        public string MainImage { get; set; }
        public Guid? AreaID { get; set; }
        public bool? IsNegotiable { get; set; }
    }
}
