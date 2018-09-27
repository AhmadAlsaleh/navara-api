using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class UpdateAdViewModel
    {
        public Guid AdID { get; set; }
        public List<string> DeletedImages { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Price { get; set; }
        public List<string> Photos { get; set; }
        public string Description { get; set; }
        public string MainImage { get; set; }
        public Guid? AreaID { set; get; }
        public Guid? CityID { set; get; }
        public Guid? CountryID { set; get; }
        public Guid? CategoryLevel1ID { set; get; }
        public Guid? CategoryLevel2ID { set; get; }
        public Guid? CategoryLevel3ID { set; get; }
        public Guid? UserID { get; set; }
        public List<CategoryFieldViewModel> CategoryFields { get; set; }
    }
}
