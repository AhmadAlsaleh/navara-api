using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NavaraAPI.ViewModels;
using SmartLifeLtd.API;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Data.Tables.Shared;
using SmartLifeLtd.Enums;
using SmartLifeLtd.Management.Interfaces;

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
                var data = _context.Set<ItemCategory>()?.Where(s=>!s.Name.Contains("Used Items")).ToList();
                var json = new JsonResult(data.Select(x => new ViewModels.ItemCategoryModel()
                {
                    ID = x.ID,
                    Name = x.Name,
                    Name2=x.Name2,
                    Description = x.Description,
                    ImagePath = x.ImagePath
                }));
                #region Add Open View History
                var clickContext = _context as IClickHistoryContext;
                if (clickContext != null)
                {
                    var clickView = clickContext.OpenViewHistories.FirstOrDefault(x =>
                        x.View == NavaraView.Categories.ToString() &&
                        x.Date.GetValueOrDefault().Date == DateTime.Now.Date);
                    if (clickView == null)
                    {
                        clickView = new OpenViewHistory()
                        {
                            ClickTime = 0,
                            View = NavaraView.Categories.ToString(),
                            Date = DateTime.Now.Date
                        };
                        clickContext.OpenViewHistories.Add(clickView);
                    }
                    clickView.ClickTime++;
                    clickContext.SubmitAsync();
                }
                #endregion
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
