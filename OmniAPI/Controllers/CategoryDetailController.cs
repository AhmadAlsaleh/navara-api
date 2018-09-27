using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OmniAPI.Models;
using SmartLifeLtd.Data.DataContexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Omni.Controllers.API
{
    [Route("api/[controller]/[Action]")]
    public class CategoryDetailController : Controller
    {
        private readonly OmniDbContext _context;

        public CategoryDetailController(OmniDbContext context)
        {
            _context = context;
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetInfo(Guid ID)
        {
            try
            {
                var item = _context.Categories.Include("Ads").Include("SubCategories").Include("Parent")
                    .Include("Parent.Parent").SingleOrDefault(x => x.ID == ID);
                if (item == null) return NoContent();
                var model = new CategoryViewModel()
                {
                    ID = item.ID,
                    Name = item.Name,
                    AdsNumber=item?.ADs?.Count,
                    ImagePath = item.ImagePath,
                    SubCategories = item.SubCategories != null ? item.SubCategories.Select(y => new CategoryViewModel() { Name = y.Name, ImagePath = y.ImagePath, ID = y.ID }).ToList() : null,
                    Parent = item.Parent != null ? new CategoryViewModel() { Name = item.Parent.Name, ImagePath = item.Parent.ImagePath, ID = item.Parent.ID } : null,
                    GrandParent = item?.Parent?.Parent != null ? new CategoryViewModel() { Name = item.Parent.Parent.Name, ImagePath = item.Parent.Parent.ImagePath, ID = item.Parent.Parent.ID } : null
                };
                return Json(model);
            }
            catch(Exception ex)
            {
                return NoContent();
            }
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetAds(Guid ID)
        {
            try
            {
                var items = _context.ADs.Include("AdImages").Include("AdImages").Include("Account")?.Include("Category").Include("Category.Parent")
                       .Include("Category.Parent.Parent")
                   .Include("Currency").Include("Area").Include("Area.City").Where(x => (x.CategoryID == ID) ||
                  (x.Category != null && x.Category.ParentID == ID) ||
                  (x.Category != null && x.Category.Parent != null && x.Category.Parent.ParentID == ID)).ToList();
                var model = items.Select(x => new 
                {
                    ID = x.ID,
                    CategoryID = x.Category?.ID,
                    //Tarek CityName = x.City?.Name,
                    AreaID = x?.Area,
                    CurrencyName = x?.Currency?.Name??"SP",
                    Name = x?.Name,
                    Hearts = x?.NumberViews,
                    PublishedDate = x.PublishedDate.Year + "/" + x.PublishedDate.Month + "/" + x.PublishedDate.Day,
                    View = x?.ADViews,
                    Price = x?.Price,
                    Title = x?.Title,
                    IsDisabled = x?.IsDisabled,
                    MainImage = x?.ADImages?.SingleOrDefault(y => y.IsMain == true)?.ImagePath??"/images/No-image-found.jpg"
                });
                return Ok(new { Ads = model.ToList() });
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }
    }
}
