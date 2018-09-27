using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using SmartLifeLtd.Data.Tables.Omni;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Classes;
using Microsoft.EntityFrameworkCore;

namespace OmniAPI.Controllers
{
    [Route("api/[controller]")]
    public class AdController : BaseController<AD>
    {
        public AdController(OmniDbContext context, LogDbContext logContext, UserManager<ApplicationUser> userManager)
            : base(context, logContext, userManager)
        {

        }
        [HttpGet]
        public override async Task<IActionResult> Get()
        {
            string DefCurrency = _context.Currencies.FirstOrDefault(x => x.IsDefault == true)?.Symbol ?? "";
            var Items = _context.ADs.Include("AdImages").Include("Account")?.Include("Category")
                   .Include("Currency").Include("Area").Include("Area.City")
                   .Select(x => new
                   {
                       ID = x.ID,
                       CategoryID = x.Category.ID,
                       //Tarek  CityName = x.City.Name,
                       CurrencyName = (x.Currency == null ? null : x.Currency.Symbol) ?? DefCurrency,
                       Name = x.Name,
                       Hearts = x.NumberViews,
                       View = x.ADViews,
                       Price = x.Price,
                       Title = x.Title,
                       MainImage = x.ADImages.FirstOrDefault(y => y.IsMain == true).ImagePath
                   });
            if (Items != null)
            {
                var json = new JsonResult(Items);
                return json;
            }
            return BadRequest();
        }

        [HttpGet("{ID}")]

        public override async Task<IActionResult> GetById(Guid ID)
        {
            try
            {
                string DefCurrency = _context.Currencies.FirstOrDefault(x => x.IsDefault == true)?.Symbol ?? "";

                double RemoveNull = new double();
                var Items = _context.ADs.Include("AdExtraFields").Include("AdExtraFields.CategoryField").Include("AdImages").Include("Account")?.Include("Category")
                   .Include("Currency").Include("Area").Include("Area.City").Include("Area.City.Country")
                .Select(x => new
                {
                    ID = x.ID,
                    Views = x.ADViews,
                    AreaName = x.Area,
                    CategoryName = x.Category.Name,
                    //Tarek CountryName = x.City.Country.Name??"",
                    //Tarek CityName = x.Area.City.Name??"",
                    Code = x.Code,
                    CurrencyName = (x.Currency == null ? null : x.Currency.Symbol) ?? DefCurrency,
                    Description = x.Description,
                    Name = x.Name,
                    Phone =x.Phone,
                    Currency=x.Currency.Code??"sp",
                    Hearts = x.NumberViews,
                    Price = x.Price,
                    //Tarek  Longitude= x.Area.longitude?? RemoveNull,
                    //Tarek latitude = x.Area.latitude?? RemoveNull,
                    PublishedDate = x.PublishedDate.Year+"/"+x.PublishedDate.Month+"/"+x.PublishedDate.Day,
                    Title = x.Title,
                    UpdatedDate = x.UpdatedDate,
                    UserID = x.AccountID,
                    UserName = x.Account.Name,
                    userImagePath=x.Account.ImagePath?? $"/images/defaultPerson.gif",
                    CategoryFields = x.ADExtraFields.Select(a => new
                    {
                        CategoryFieldID = a.CategoryFieldOptionID,
                        //Tarek  Type = a.CategoryFieldOption.Type,
                        Name = a.CategoryFieldOption.Name,
                        Value = a.Value
                    }),
                        Images = x.ADImages.Select(c => c.ImagePath).ToList()
                }).SingleOrDefault(c => c.ID == ID);
                if (Items != null)
                {
                    var Item = _context.ADs?.SingleOrDefault(s => s.ID == Items.ID);
                    Item.ADViews++;
                    _context.SubmitAsync();
                    var json = new JsonResult(Items);
                    return json;
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
               
            }
            
            return BadRequest();
        }
    }
}
