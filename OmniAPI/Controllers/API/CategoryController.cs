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

namespace OmniAPI.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : BaseController<Category>
    {
        public CategoryController(OmniDbContext context, LogDbContext logContext, UserManager<ApplicationUser> userManager)
            : base(context, logContext, userManager)
        {

        }
        
        public override async Task<IActionResult> Get()
        {
            try
            {
                AD ad = new AD();
                var cat = _context?.Categories?.Include("Ads")?.Include("Ads.Category")
                    ?.Include("Ads.Category.Parent")?.Include("CategoryFields")?
                    .Include("CategoryFields.CategoryFieldLanguages")?
                    .Include("CategoryFields.CategoryFieldLanguages.Language")?
                    .Include("CategoryLanguages")?.Include("CategoryLanguages.Language")?
                    .ToList()?.Select(x => new
                {
                    ID = x.ID,
                    AdsNumber=_context.ADs.Where(s=>s.IsDisabled==false)
                     .Where(a => (a.CategoryID == x.ID) ||
                  (a.Category != null && a.Category.ParentID == x.ID) ||
                  (a.Category != null && a.Category.Parent != null && a.Category.Parent.ParentID == x.ID)).ToList().Count,
                    ImagePath = x.ImagePath,
                    Name = x.Name,
                    color=x.ColorCode,
                    ParentID = x.ParentID,
                    hasChildren = x.HasChildren,
                    CategoryLanguages = x.CategoryLanguages.Select(a=>new {Code =a.Language?.Code, a.Name }).ToList(),
                    CategoryFields = x.CategoryFields.Select(a => new { CategoryFieldID=a.ID, Type =a.Type,Name=a.Name,
                    CategoryFieldLanguages = a.CategoryFieldLanguages.Select(s=>new { Code = s.Language?.Code,s.Name }).ToList() }).ToList(),


                });
                if (cat == null)
                {
                    return BadRequest("Empty");
                }
                return Ok(cat);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
             
            }
            
       
        }
    }
}
