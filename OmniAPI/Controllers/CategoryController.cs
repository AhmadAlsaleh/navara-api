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
using OmniAPI.Classes;

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

                var adCategories = _context.Set<AD>()
                    .Include(x => x.Category)
                        .ThenInclude(x => x.Parent)
                            .ThenInclude(x => x.Parent)
                    .Select(x => new
                    {
                        Category = x.Category,
                        Parent = x.Category == null ? null : x.Category.Parent,
                        GrandParent = x.Category == null ? null : x.Category.Parent == null ? null : x.Category.Parent.Parent
                    }).ToList();

                var cat = categories.Select(x => new CategoryDataModel
                {
                    ID = x.ID,
                    ImagePath = x.ImagePath,
                    Name = x.Name,
                    ParentID = x.ParentID,
                    Color = x.ColorCode,
                    AdsNumber = adCategories.Count(y => y.Category?.ID == x.ID || y.Parent?.ID == x.ID || y.GrandParent?.ID == x.ID)
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
                    .Include(x => x.CategoryFields)
                        .ThenInclude(x => x.CategoryFieldOptions)
                    .Include(x => x.SubCategories)
                    .Include(x => x.Parent)
                        .ThenInclude(x => x.Parent)
                    .SingleOrDefaultAsync(x => x.ID == id);
                if (item == null) return BadRequest("No Category related to this ID");

                var AdsNumber = _context.Set<AD>()
                 .Where(s => (s.IsDisabled ?? false) == false)
                 .Where(a => (a.CategoryID == item.ID) ||
                     (a.Category != null && a.Category.ParentID == item.ID) ||
                     (a.Category != null && a.Category.Parent != null && a.Category.Parent.ParentID == item.ID)).Count();

                var json = new JsonResult(new CategoryFullDataModel()
                {
                    ID = item.ID,
                    Name = item.Name,
                    AdsNumber = AdsNumber,
                    ImagePath = item.ImagePath,
                    Color = item.ColorCode,
                    HasChildren = item.HasChildren,
                    ParentID = item.ParentID,
                    SubCategories = item.SubCategories.Select(y => new CategoryDataModel()
                    {
                        ID = y.ID,
                        ParentID = y.ParentID,
                        Name = y.Name,
                        ImagePath = y.ImagePath,
                        Color = y.ColorCode
                    }).ToList(),
                    Parent = item.Parent == null ? null : new CategoryDataModel()
                    {
                        ID = item.Parent.ID,
                        ParentID = item.ParentID,
                        Name = item.Parent.Name,
                        ImagePath = item.Parent.ImagePath,
                        Color = item.ColorCode
                    },
                    GrandParent = item?.Parent?.Parent == null ? null : new CategoryDataModel()
                    {
                        ID = item.Parent.Parent.ID,
                        ParentID = item.ParentID,
                        Name = item.Parent.Parent.Name,
                        ImagePath = item.Parent.Parent.ImagePath,
                        Color = item.ColorCode,
                    },
                    categoryFields = item.CategoryFields.Select(a => new CategoryFieldDataModel
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
                    }).ToList(),
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
                    .Include(x => x.FavouriteADs)
                    .Include(x => x.Currency)
                    .Include(x => x.Category)
                        .ThenInclude(x => x.Parent)
                            .ThenInclude(x => x.Parent)
                   .Where(x =>
                        (x.CategoryID == ID) ||
                        (x.Category != null && x.Category.ParentID == ID) ||
                        (x.Category != null && x.Category.Parent != null && x.Category.Parent.ParentID == ID)).ToList();

                var data = items.Select(x => new ADDataModel
                {
                    ID = x.ID,
                    CategoryID = x.Category?.ID,
                    Name = x.Name,
                    Likes = x.FavouriteADs.Count,
                    PublishedDate = x.PublishedDate,
                    Views = x.ADViews,
                    Price = x.Price,
                    Title = x.Title,
                    Code = x.Code,
                    MainImage = x.GetMainImageRelativePath(),
                    Category = x.Category?.Name,
                    Currency = x.Currency?.Name
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
