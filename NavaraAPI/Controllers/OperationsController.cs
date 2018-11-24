using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.ViewModels;
using Newtonsoft.Json;
using SmartlifeLtd.Data.Tables.Navara;
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
                    return Json(JsonConvert.SerializeObject(Courses));
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
                    return Json(JsonConvert.SerializeObject(Organiztaions));
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
        //[AuthorizeToken]
        //[HttpPost]
        //public async Task<IActionResult> NewOrderdItem([FromBody]ItemNewModel model)
        //{
            //try
            //{
            //    if (model == null) return BadRequest("Error while recieving data");
            //    var userID = HttpContext.User.Identity.Name;
            //    if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            //    ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(x => x.UserName == userID);
            //    if (user == null) return BadRequest("Token is not related to any Account");
            //    Account account = _context.Set<Account>().Include(x => x.Cart)
            //        .ThenInclude(x => x.CartItems).FirstOrDefault(x => x.ID == user.AccountID);
            //    if (user == null || account == null) return null;
            //    ItemCategory cat = _context.Set<ItemCategory>().FirstOrDefault(x => x.IsUsed == true);
            //    if (cat == null)
            //        return StatusCode(StatusCodes.Status503ServiceUnavailable);

            //    Item item = new Item()
            //    {
            //        CashBack = 0,
            //        CreationDate = DateTime.Now,
            //        ItemCategoryID = cat.ID,
            //        IsEnable = true,

            //        Price = model.Price,
            //        AccountID = account.ID,
            //        Name = model.Name,
            //        Quantity = model.Quantity,
            //        Description = model.Description,
            //        Owner = model.Owner,
            //        Mobile = model.Mobile,
            //        Location = model.Location,
            //        ShortDescription = model.Description.Substring(0, Math.Min(25, model.Description.Length))
            //    };
            //    #region Save Main Image
            //    if (model.Thumbnail != null && model.Thumbnail.Length > 0)
            //    {
            //        item.ThumbnailImagePath = ImageOperations.SaveItemImage(model.Thumbnail, item);
            //        var itemImage = new ItemImage() { Item = item };
            //        itemImage.ImagePath = ImageOperations.SaveItemImage(model.Thumbnail, itemImage);
            //        if (!string.IsNullOrWhiteSpace(itemImage.ImagePath)) { item.ItemImages.Add(itemImage); }
            //        else itemImage.Item = null;
            //    }
            //    #endregion

            //    #region Save Other images
            //    if (model.Images != null && model.Images.Any(x => x.Length > 0))
            //    {
            //        foreach (var file in model.Images.Where(x => x.Length > 0))
            //        {
            //            var itemImage = new ItemImage()
            //            {
            //                Item = item
            //            };
            //            itemImage.ImagePath = ImageOperations.SaveItemImage(file, itemImage);
            //            if (!string.IsNullOrWhiteSpace(itemImage.ImagePath))
            //            {
            //                item.ItemImages.Add(itemImage);
            //            }
            //            else itemImage.Item = null;
            //        }
            //    }
            //    #endregion

            //    var data = _context.Set<Item>().Add(item);
            //    _context.SaveChanges();
            //    return Ok(item.ID);
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest(ex.Message);
            //}
      //  }
        public ActionResult Chat()
        {
            return View("~/views/Index.cshtml");
        }
        //

    }
}
