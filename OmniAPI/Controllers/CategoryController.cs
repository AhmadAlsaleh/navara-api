using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLifeLtd.Data;
using System.Linq;
using System.Threading.Tasks;
using OmniAPI.Controllers;
using SmartLifeLtd.Data.Tables.Omni;using SmartLifeLtd.Data.AspUsers;using SmartLifeLtd.Data.DataContexts;
using System;
using System.IO;
using OmniAPI.Models;
using SmartLifeLtd.API;

namespace OmniAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class CategoryController : BaseController<Category>
    {
        public CategoryController(OmniDbContext context, LogDbContext logContext, UserManager<ApplicationUser> userManager)
            : base(context)
        {

        }

        public override async Task<IActionResult> Get()
        {
            try
            {
                AD ad = new AD();
                var categories = _context.Set<Category>()
                    .Include(x => x.CategoryFields)
                        .ThenInclude(x => x.CategoryFieldLanguages)
                            .ThenInclude(x => x.Language)
                    .Include(x => x.CategoryFields)
                        .ThenInclude(x => x.CategoryFieldOptions)
                    .Include(x => x.CategoryLanguages)
                        .ThenInclude(x => x.Language)
                    .ToList();
                var cat = categories?.Select(x => new CategoryFullDataModel
                {
                    ID = x.ID,
                    ImagePath = x.ImagePath,
                    Name = x.Name,
                    ParentID = x.ParentID,
                    Color = x.ColorCode,
                    HasChildren = x.HasChildren,
                    AdsNumber = _context.Set<AD>()
                                     .Where(s => (s.IsDisabled ?? false) == false)
                                     .Where(a => (a.CategoryID == x.ID) ||
                                         (a.Category != null && a.Category.ParentID == x.ID) ||
                                         (a.Category != null && a.Category.Parent != null && a.Category.Parent.ParentID == x.ID)).Count(),
                    categoryFields = x.CategoryFields.Select(a => new CategoryFieldDataModel
                    {
                        CategoryFieldID = a.ID,
                        CategoryFieldType = a.Type,
                        CategoryFieldName = a.Name,
                        CategoryFieldOption = a.CategoryFieldOptions.Select(opt => new CategoryFieldOptionDataModel()
                        {
                            CategoryFieldID = a.ID,
                            CategoryFieldOptionID = opt.ID,
                            Value = opt.Name
                        }).ToList()
                    }).ToList()
                });
                return Ok(cat);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{id}")]
        public override async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var item = await _context.Set<Category>().Include(x => x.ADs)
                    .Include(x => x.SubCategories)
                    .Include(x => x.Parent)
                        .ThenInclude(x => x.Parent)
                    .SingleOrDefaultAsync(x => x.ID == id);
                if (item == null) return BadRequest();
                var json = new JsonResult(new CategoryDataModel()
                {
                    ID = item.ID,
                    Name = item.Name,
                    AdsNumber = item?.ADs?.Count,
                    ImagePath = item.ImagePath,
                    SubCategories = item.SubCategories != null ? item.SubCategories.Select(y => new CategoryDataModel() { Name = y.Name, ImagePath = y.ImagePath, ID = y.ID }).ToList() : null,
                    Parent = item.Parent != null ? new CategoryDataModel() { Name = item.Parent.Name, ImagePath = item.Parent.ImagePath, ID = item.Parent.ID } : null,
                    GrandParent = item?.Parent?.Parent != null ? new CategoryDataModel() { Name = item.Parent.Parent.Name, ImagePath = item.Parent.Parent.ImagePath, ID = item.Parent.Parent.ID } : null
                });
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetAds(Guid ID)
        {
            try
            {
                var items = _context.Set<AD>()
                    .Include(x => x.ADImages)
                    .Include(x => x.Account)
                    .Include(x => x.Category)
                        .ThenInclude(x => x.Parent)
                            .ThenInclude(x => x.Parent)
                   .Include(x => x.Currency)
                   .Where(x =>
                        (x.CategoryID == ID) ||
                        (x.Category != null && x.Category.ParentID == ID) ||
                        (x.Category != null && x.Category.Parent != null && x.Category.Parent.ParentID == ID)).ToList();
                var data = items.Select(x => new ADDataModel
                {
                    ID = x.ID,
                    CategoryID = x.Category?.ID,
                    Name = x.Name,
                    Likes = x.NumberViews,
                    PublishedDate = x.PublishedDate,
                    Views = x.ADViews,
                    Price = x.Price,
                    Title = x.Title,
                    Code = x.Code,
                    MainImage = x.ADImages.SingleOrDefault(y => y.IsMain == true)?.ImagePath ?? "/images/No-image-found.jpg"
                });
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
