using SmartLifeLtd.Data.Tables.Omni;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class ADFullDataModel
    {
        public Guid? ID { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public double? Price { get; set; }
        public bool? IsOwner { get; set; }

        public Guid? CategoryID { get; set; }
        public string CategoryName  { get; set; }
        public string CurrencyName { get; set; }
        public DateTime PublishedDate { get; set; }
        public string MainImage { get; set; }
        public bool? IsNegotiable { get; set; }

        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

        public int? Likes { get; set; }
        public int? Views { get; set; }

        public List<CategoryFieldOptionDataModel> CategoryFieldOptions { get; set; }
        public List<string> Images { get; set; }
    }

    public class ADDataModel
    {
        public Guid? ID { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public double? Price { get; set; }
        public string Currency { get; set; }
        public bool? IsOwner { get; set; }

        public Guid? CategoryID { get; set; }
        public string Category { get; set; }
        public DateTime PublishedDate { get; set; }
        public string MainImage { get; set; }

        public int? Likes { get; set; }
        public int? Views { get; set; }
    }
}
