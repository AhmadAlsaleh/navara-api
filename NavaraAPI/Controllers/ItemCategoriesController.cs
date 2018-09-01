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
    [Route("[controller]")]
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
                var json = new JsonResult(data.Select(x => new ItemCategoryViewModel()
                {
                    Name = x.Name,
                    Description = x.Description
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
