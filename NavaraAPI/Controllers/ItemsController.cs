using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.ViewModels;
using SmartLifeLtd.API;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;

namespace NavaraAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class ItemsController : BaseController<Item>
    {
        public ItemsController(NavaraDbContext context)
            : base(context)
        {

        }

        [HttpGet("{id}")]
        [ActionName("Get")]
        public override async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var item = await _context.Set<Item>().Include(x => x.ItemCategory)
                    .Include(x => x.ItemImages).FirstOrDefaultAsync(x => x.ID == id);
                if (item == null) return NotFound("id is not realted to any Item");
                var json = new JsonResult(new ItemFullModel()
                {
                    ID = item.ID,
                    Name = item.Name,
                    ShortDescription = item.ShortDescription,
                    ItemCategoryID = item.ItemCategoryID,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Description = item.Description,
                    ThumbnailImagePath = item.ThumbnailImagePath,
                    IsEnable = item.IsEnable,
                    ItemCategory = item.ItemCategory?.Name,
                    ItemImages = item.ItemImages.Select(y => y.ImagePath).ToList()
                });
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBasic()
        {
            try
            {
                var data = _context.Set<Item>().Include(x => x.ItemCategory).ToList();
                var json = new JsonResult(data.Select(x => new ItemBasicModel()
                {
                    ID = x.ID,
                    Name = x.Name,
                    ShortDescription = x.ShortDescription,
                    ItemCategory = x.ItemCategory?.Name,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    ThumbnailImagePath = x.ThumbnailImagePath
                }));
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBasicByCategory(Guid id)
        {
            try
            {
                var data = _context.Set<Item>().Include(x => x.ItemCategory)
                    .Where(x => x.ItemCategoryID == id).ToList();
                var json = new JsonResult(data.Select(x => new ItemBasicModel()
                {
                    ID = x.ID,
                    Name = x.Name,
                    ShortDescription = x.ShortDescription,
                    ItemCategory = x.ItemCategory?.Name,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    ThumbnailImagePath = x.ThumbnailImagePath
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
