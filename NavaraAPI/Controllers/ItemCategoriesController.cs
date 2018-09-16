using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NavaraAPI.ViewModels;
using SmartLifeLtd.API;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;

namespace NavaraAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class ItemCategoriesController : BaseController<ItemCategory>
    {
        public ItemCategoriesController(NavaraDbContext context)
            : base(context)
        {

        }

        [HttpGet]
        public override async Task<IActionResult> Get()
        {
            try
            {
                var data = _context.Set<ItemCategory>().ToList();
                var json = new JsonResult(data.Select(x => new ViewModels.ItemCategoryModel()
                {
                    ID = x.ID,
                    Name = x.Name,
                    Description = x.Description,
                    ImagePath = x.ImagePath
                }));
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
