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
    public class ItemsController : BaseController<Item>
    {
        public ItemsController(NavaraDbContext context)
            : base(context)
        {

        }

        [HttpGet]
        public override async Task<IActionResult> Get()
        {
            try
            {
                var data = _context.Set<Item>().ToList();
                var json = new JsonResult(data.Select(x => new ItemViewModel()
                {
                    Name = x.Name,
                    Description = x.Description,
                    ItemCategoryID = x.ItemCategoryID,
                    Price = x.Price,
                    Quantity = x.Quantity
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
