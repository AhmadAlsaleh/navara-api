using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.ViewModels;
using SmartLifeLtd.API;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Data.Tables.Shared;
using SmartLifeLtd.Enums;
using SmartLifeLtd.Management.Interfaces;

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
                    CashBack = item.CashBack,
                    Quantity = item.Quantity,
                    Description = item.Description,
                    ThumbnailImagePath = item.ThumbnailImagePath,
                    IsEnable = item.IsEnable,
                    ItemCategory = item.ItemCategory?.Name,
                    ItemImages = item.ItemImages.Select(y => y.ImagePath).ToList(),
                    DaysToBeAvilable = item.DaysToBeAvailable,

                    AccountID = item.AccountID,
                    Location = item.Location,
                    Mobile = item.Mobile,
                    Owner = item.Owner
                });
                #region Add Click History
                var clickContext = _context as IClickHistoryContext;
                if (clickContext != null)
                {
                    var clickObject = clickContext.ObjectClicks.FirstOrDefault(x => x.ObjectType == nameof(Item) && x.ObjectID == id);
                    if (clickObject == null)
                    {
                        clickObject = new SmartLifeLtd.Data.Tables.Shared.ObjectClick()
                        {
                            ObjectType = nameof(Item),
                            ObjectID = id,
                            ClicksCount = 0
                        };
                        clickContext.ObjectClicks.Add(clickObject);
                    }
                    clickObject.ClicksCount++;
                    clickObject.LastClickedDate = DateTime.UtcNow;
                    _context.SaveChanges();
                }
                #endregion
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
                var data = _context.Set<Item>().Include(x => x.ItemCategory)
                    .Where(x => x.IsEnable == true).ToList();
                var json = new JsonResult(data.Select(x => new ItemBasicModel()
                {
                    ID = x.ID,
                    Name = x.Name,
                    ShortDescription = x.ShortDescription,
                    ItemCategory = x.ItemCategory?.Name,
                    Price = x.Price,
                    CashBack = x.CashBack,
                    Quantity = x.Quantity,
                    ItemCategoryID = x.ItemCategoryID,
                    ThumbnailImagePath = x.ThumbnailImagePath,
                    AccountID = x.AccountID,
                    IsEnable = x.IsEnable,
                    DaysToBeAvilable = x.DaysToBeAvailable
                }));
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetUsedITems()
        {
            try
            {
                var data = _context.Set<Item>().Include(x => x.ItemCategory)
                    .Where(x => x.IsEnable == true&&x.ItemCategory.Name.Contains("Used Items")).ToList();
                var json = new JsonResult(data.Select(x => new ItemBasicModel()
                {
                    ID = x.ID,
                    Name = x.Name,
                    ShortDescription = x.ShortDescription,
                    ItemCategory = x.ItemCategory?.Name,
                    Price = x.Price,
                    CashBack = x.CashBack,
                    Quantity = x.Quantity,
                    ItemCategoryID = x.ItemCategoryID,
                    ThumbnailImagePath = x.ThumbnailImagePath,
                    AccountID = x.AccountID,
                    IsEnable = x.IsEnable,
                    DaysToBeAvilable = x.DaysToBeAvailable
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
                    .Where(x => x.ItemCategoryID == id && x.IsEnable == true).ToList();
                var json = new JsonResult(data.Select(x => new ItemBasicModel()
                {
                    ID = x.ID,
                    Name = x.Name,
                    ShortDescription = x.ShortDescription,
                    ItemCategory = x.ItemCategory?.Name,
                    Price = x.Price,
                    CashBack = x.CashBack,
                    Quantity = x.Quantity,
                    ItemCategoryID = x.ItemCategoryID,
                    ThumbnailImagePath = x.ThumbnailImagePath,
                    AccountID = x.AccountID,
                    IsEnable = x.IsEnable,
                    DaysToBeAvilable = x.DaysToBeAvailable
                }));

                #region Add Open View History
                var clickContext = _context as IClickHistoryContext;
                if (clickContext != null)
                {
                    var cat = _context.Set<ItemCategory>().FirstOrDefault(x => x.ID == id)?.Name;
                    var clickView = clickContext.OpenViewHistories.FirstOrDefault(x =>
                        x.View == NavaraView.Item.ToString() &&
                        x.Date.GetValueOrDefault().Date == DateTime.Now.Date &&
                        x.Remark == cat);
                    if (clickView == null)
                    {
                        clickView = new OpenViewHistory()
                        {
                            ClickTime = 0,
                            View = NavaraView.Item.ToString(),
                            Date = DateTime.Now.Date,
                            Remark = cat
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

        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> AddNew([FromBody]ItemNewModel model)
        {
            try
            {
                if (model == null) return BadRequest("Error while recieving data");
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(x => x.UserName == userID);
                if (user == null) return BadRequest("Token is not related to any Account");
                Account account = _context.Set<Account>().Include(x => x.Cart)
                    .ThenInclude(x => x.CartItems).FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                ItemCategory cat = _context.Set<ItemCategory>().FirstOrDefault(x => x.IsUsed == true);
                if (cat == null)
                    return StatusCode(StatusCodes.Status503ServiceUnavailable);

                Item item = new Item()
                {
                    CashBack = 0,
                    CreationDate = DateTime.Now,
                    ItemCategoryID = cat.ID,
                    IsEnable = true,

                    Price = model.Price,
                    AccountID = account.ID,
                    Name = model.Name,
                    Quantity = model.Quantity,
                    Description = model.Description,
                    Owner = model.Owner,
                    Mobile = model.Mobile,
                    Location = model.Location,
                    ShortDescription = model.Description.Substring(0, Math.Min(25, model.Description.Length))
                };
                #region Save Main Image
                if (model.Thumbnail != null && model.Thumbnail.Length > 0)
                {
                    item.ThumbnailImagePath = ImageOperations.SaveItemImage(model.Thumbnail, item);
                    var itemImage = new ItemImage() { Item = item };
                    itemImage.ImagePath = ImageOperations.SaveItemImage(model.Thumbnail, itemImage);
                    if (!string.IsNullOrWhiteSpace(itemImage.ImagePath)) { item.ItemImages.Add(itemImage); }
                    else itemImage.Item = null;
                }
                #endregion

                #region Save Other images
                if (model.Images != null && model.Images.Any(x => x.Length > 0))
                {
                    foreach (var file in model.Images.Where(x => x.Length > 0))
                    {
                        var itemImage = new ItemImage()
                        {
                            Item = item
                        };
                        itemImage.ImagePath = ImageOperations.SaveItemImage(file, itemImage);
                        if (!string.IsNullOrWhiteSpace(itemImage.ImagePath))
                        {
                            item.ItemImages.Add(itemImage);
                        }
                        else itemImage.Item = null;
                    }
                }
                #endregion

                var data = _context.Set<Item>().Add(item);
                _context.SaveChanges();
                return Ok(item.ID);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
