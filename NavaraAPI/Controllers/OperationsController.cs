using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.ViewModels;
using Newtonsoft.Json;
using SmartlifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Data.Tables.Shared;
using SmartLifeLtd.Enums;
using SmartLifeLtd.Management.Interfaces;
using SmartLifeLtd.Sync;
using SmartLifeLtd.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class OperationsController : SmartLifeLtd.Controllers.BaseOperationsController
    {
        public OperationsController(NavaraDbContext context, IHostingEnvironment env) : base(context, env)
        {
        }

        [HttpGet]
        public async Task<ActionResult> GetCourses()
        {
            try
            {
                var Courses = _Context.Set<Course>()?.Include(s => s.Organiztaion)?.Where(s => s.IsActive == true).Select(a => new
                {
                    a.Title,
                    a.Cost,
                    a.Description,
                    a.StartDate,
                    OrganiztaionName = a.Organiztaion.Name,
                    ContactName = a.Organiztaion.ContactName,
                    a.IsActive,
                    a.SessionsNumber
                }
                        );
                if (Courses != null)
                {
           
                    return Json(Courses);
                }
                else
                {
                    return BadRequest("Empty");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);

            }
    
        }
        [HttpGet]
        public async Task<ActionResult> GetEvents()
        {
            try
            {
                var Events = _Context.Set<Event>()?.Include(s => s.Organiztaion)?.Where(s => s.IsActive == true).Select(a => new
                {
                    a.Title,
                    a.Description,
                    a.StartDate,
                    OrganiztaionName = a.Organiztaion.Name,
                    ContactName = a.Organiztaion.ContactName,
                    a.IsActive,
                    a.SessionsNumber
                }
                        );
                if (Events != null)
                {
         
                    return Json(Events);
                }
                else
                {
                    return BadRequest("Empty");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);

            }

        }

        [HttpGet]
        public async Task<ActionResult> GetOrganiztaion()
        {
            try
            {
                var Organiztaions = _Context.Set<Organiztaion>()?.Select(a => new
                {
                    a.Name,
                    a.LogoPath,
                    a.Description,
                    a.Phone,
                    a.ContactName,
                    a.Location
                }
                        );
                if (Organiztaions != null)
                {
                    return Json( Organiztaions );
                }
                else
                {
                    return BadRequest("Empty");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);

            }

        }
        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> NewOrderedItem([FromBody]OrderedItemViewModel model)
        {
            try
            {
                if (model == null) return BadRequest("Error while recieving data");
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _Context.Set<ApplicationUser>().SingleOrDefaultAsync(x => x.UserName == userID);
                if (user == null) return BadRequest("Token is not related to any Account");
                Account account = _Context.Set<Account>().Include(x => x.Cart)
                    .ThenInclude(x => x.CartItems).FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;

                OrderdItem Ordereditem = new OrderdItem()
                {
                    CreationDate = DateTime.Now,
                    NeededDate = model.NeededDate,
                    Name = model.Name,
                    Description = model.Description,
                    AccountID = account?.ID,
                };
                #region Save Other images
                if (model.Images != null && model.Images.Any(x => x.Length > 0))
                {
                    foreach (var file in model.Images.Where(x => x.Length > 0))
                    {
                        var itemImage = new OrderdItemImage()
                        {
                            OrderdItem = Ordereditem
                        };
                        itemImage.ImagePath = ImageOperations.SaveItemImage(file, itemImage);
                        if (!string.IsNullOrWhiteSpace(itemImage.ImagePath))
                        {
                            Ordereditem.OrderdItemImages.Add(itemImage);
                        }
                        else itemImage.OrderdItem = null;
                    }
                }
                #endregion

                var data = _Context.Set<OrderdItem>().Add(Ordereditem);
                _Context.SaveChanges();
                return Ok(Ordereditem.ID);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public ActionResult Chat()
        {
            return View("~/views/Index.cshtml");
        }
        //

    }
}
