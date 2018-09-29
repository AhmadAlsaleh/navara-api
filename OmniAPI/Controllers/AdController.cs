using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OmniAPI.Controllers;
using OmniAPI.Models;
using SmartLifeLtd.API;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Omni;
using SmartLifeLtd.Data.Tables.OMNI;
using SmartLifeLtd.Data.Tables.Shared;
using SmartLifeLtd.Enums;
using SmartLifeLtd.Sync;
using SmartLifeLtd.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Omni.Controllers.API
{
    [Route("[controller]/[action]")]
    public class ItemController : BaseController<AD>
    {
        public ItemController(UserManager<ApplicationUser> userManager, OmniDbContext context, LogDbContext logContext)
            : base(context)
        {
        }

        [HttpGet]
        public override async Task<IActionResult> Get()
        {
            string DefCurrency = _context.Set<Currency>().FirstOrDefault(x => x.IsDefault == true)?.Symbol ?? "S.P";
            var Items = await _context.Set<AD>()
                        .Include(x => x.ADImages)
                        .Include(x => x.Account)
                        .Include(x => x.Category)
                        .Include(x => x.Currency).Skip(0).Take(10).ToListAsync();
            var returnedData = Items.Select(x => new ADDataModel
            {
                ID = x.ID,
                Name = x.Name,
                CategoryID = x.Category?.ID,
                Code = x.Code,
                PublishedDate = x.PublishedDate,
                Likes = x.NumberViews,
                Views = x.ADViews,
                Price = x.Price,
                Title = x.Title,
                MainImage = x.ADImages.FirstOrDefault(y => y.IsMain == true)?.ImagePath
            });
            return Json(returnedData);
        }

        [HttpGet("{ID}")]
        public override async Task<IActionResult> GetById(Guid ID)
        {
            try
            {
                var Item = _context.Set<AD>()
                         .Include(x => x.ADExtraFields)
                             .ThenInclude(x => x.CategoryFieldOption)
                         .Include(x => x.Account)
                         .Include(x => x.Category)
                         .Include(x => x.Currency).SingleOrDefault(c => c.ID == ID);
                if (Item == null)
                    return BadRequest("No Ad related to this ID");

                return new JsonResult(new ADFullDataModel
                {
                    ID = Item.ID,
                    IsNegotiable = Item.IsNegotiable,
                    Views = Item.ADViews,
                    CategoryID = Item.CategoryID,
                    CategoryName = Item.Category?.Name,
                    Code = Item.Code,
                    CurrencyName = Item.Currency?.Name,
                    Description = Item.Description,
                    Name = Item.Name,
                    Phone = Item.Phone,
                    Likes = Item.NumberViews,
                    Price = Item.Price,
                    Longitude = Item.Longitude,
                    Latitude = Item.Latitude,
                    PublishedDate = Item.PublishedDate,
                    Email = Item.Email,
                    Title = Item.Title,
                    MainImage = Item.ADImages?.FirstOrDefault(y => y.IsMain == true)?.ImagePath,
                    Images = Item.ADImages?.Where(s => s.IsMain == false).Select(c => c.ImagePath).ToList(),
                    CategoryFieldOptions = Item.ADExtraFields.Select(a => new CategoryFieldOptionDataModel()
                    {
                        CategoryFieldID = a.CategoryFieldOption?.CategoryFieldID,
                        CategoryFieldOptionID = a.CategoryFieldOptionID,
                        Value = a.CategoryFieldOption?.Name,
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchDataModel model)
        {
            var Sort = EnumHelper<SortingType>.Parse(model.SortingType);
            var ads = _context.Set<AD>().Include(x => x.ADImages)
                .Include(x => x.Currency)
                .Include(x => x.Category)
                    .ThenInclude(x => x.Parent)
                        .ThenInclude(x => x.Parent)
                .Where(x => (x.IsDisabled ?? false) == false);
            if (!string.IsNullOrWhiteSpace(model.SearchWord))
                ads = ads.Where(x =>
                    x.Title.ToLower().Contains(model.SearchWord.ToLower()) ||
                    x.Description.ToLower().Contains(model.SearchWord.ToLower()) ||
                    x.Email.ToLower().Contains(model.SearchWord.ToLower()) ||
                    x.Category == null || x.Category.Name.ToLower().Contains(model.SearchWord.ToLower()) ||
                    x.Code.ToLower().Contains(model.SearchWord.ToLower()) ||
                    x.Name.ToLower().Contains(model.SearchWord.ToLower()) ||
                    x.Phone.ToLower().Contains(model.SearchWord.ToLower()));

            if (model.LowPrice != null)
                ads = ads.Where(x => x.Price >= model.LowPrice);

            if (model.HighPrice != null)
                ads = ads.Where(x => x.Price <= model.HighPrice);

            if (model.CategoryID != null)
            {
                ads = ads.Where(x =>
                    (x.CategoryID == model.CategoryID) ||
                    (x.Category != null && x.Category.ParentID == model.CategoryID) ||
                    (x.Category != null && x.Category.Parent != null && x.Category.Parent.ParentID == model.CategoryID));
            }
            string DefCurrency = _context.Set<Currency>().FirstOrDefault(x => x.IsDefault == true)?.Symbol ?? "SP";

            var result = ads.Select(x => new ADDataModel
            {
                ID = x.ID,
                Name = x.Name,
                Likes = x.NumberViews,
                PublishedDate = x.PublishedDate,
                Views = x.ADViews,
                Price = x.Price,
                Title = x.Title,
                MainImage = x.ADImages.FirstOrDefault(y => y.IsMain == true).ImagePath,
                CategoryID = x.CategoryID,
                Code = x.Code
            });
            List<ADDataModel> FinalResult = new List<ADDataModel>();
            switch (Sort)
            {
                case SortingType.None:
                    FinalResult = result.OrderByDescending(s => s.PublishedDate).Skip(model.Page * model.Count).Take(model.Count).ToList();
                    break;
                case SortingType.MostRecently:
                    FinalResult = result.OrderByDescending(s => s.PublishedDate).Skip(model.Page * model.Count).Take(model.Count).ToList();
                    break;
                case SortingType.HighToLowPrice:
                    FinalResult = result.OrderByDescending(s => s.Price).Skip(model.Page * model.Count).Take(model.Count).ToList();
                    break;
                case SortingType.LowToHighPrice:
                    FinalResult = result.OrderBy(s => s.Price).Skip(model.Page * model.Count).Take(model.Count).ToList();
                    break;
            }
            return Ok(FinalResult);
        }

        [AuthorizeToken]
        [HttpPost("{ID}")]
        public async Task<IActionResult> Update([FromBody] ADFullDataModel model, Guid ID)
        {
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _context.Set<Account>()
                .Include(x => x.ADs)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            var UpAd = _context.Set<AD>().Include(x => x.Category)
                .Include(x => x.Account).SingleOrDefault(s => s.ID == ID);
            if (UpAd == null) return BadRequest("No Ad related to this ID");
            try
            {
                UpAd.CategoryID = model.CategoryID;
                UpAd.Name = model.Name;
                UpAd.Title = model.Title;
                UpAd.Description = model.Description;
                UpAd.Email = model.Email;
                UpAd.Phone = model.Phone;
                UpAd.IsDisabled = false;
                UpAd.Price = model.Price ?? 0;
                UpAd.Longitude = model.Longitude;
                UpAd.Latitude = model.Latitude;
                UpAd.IsNegotiable = model.IsNegotiable;

                #region Update Category fields
                var OldFields = _context.Set<AdExtraField>().Where(s => s.AdID == UpAd.ID).ToList();
                _context.Set<AdExtraField>().RemoveRange(OldFields);

                if (model.CategoryFieldOptions?.Count > 0)
                {
                    foreach (var item in model.CategoryFieldOptions)
                    {
                        var CatFieldOption = _context?.Set<CategoryFieldOption>()?.SingleOrDefault(S => S.ID == item.CategoryFieldOptionID);
                        _context.Set<AdExtraField>().Add(new AdExtraField
                        {
                            Ad = UpAd,
                            CategoryFieldOptionID = item.CategoryFieldOptionID,
                            Value = CatFieldOption.Name
                        });
                    }
                }
                #endregion

                #region Update main images
                if (!string.IsNullOrEmpty(model.MainImage))
                {
                    var image = UpAd.ADImages.FirstOrDefault(s => s.IsMain == true);
                    if (model.MainImage?.Length > 0)
                    {
                        string filename = ImageOperations.SaveImage(model.MainImage, UpAd);
                        if (!string.IsNullOrEmpty(filename))
                        {
                            ADImage pic = new ADImage()
                            {
                                AD = UpAd,
                                ImagePath = filename,
                                IsMain = true
                            };
                            _context.Set<ADImage>().Add(pic);
                        }
                    }
                }
                #endregion

                #region Upload Other images
                if (model?.Images != null)
                {
                    foreach (var file in model.Images)
                    {
                        if (file.Length > 0)
                        {
                            string filename = ImageOperations.SaveImage(file, UpAd);
                            if (!string.IsNullOrEmpty(filename))
                            {
                                ADImage pic = new ADImage()
                                {
                                    AD = UpAd,
                                    ImagePath = filename,
                                    IsMain = false
                                };
                                _context.Set<ADImage>().Add(pic);
                            }
                        }
                    }
                }
                #endregion

                await _context.SubmitAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [AuthorizeToken]
        public async Task<IActionResult> Add([FromBody] ADFullDataModel model)
        {
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _context.Set<Account>()
                .Include(x => x.ADs)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            try
            {
                #region Create new AD
                AD newAd = new AD()
                {
                    Name = model.Name,
                    Title = model.Title,
                    Description = model.Description,
                    CategoryID = model.CategoryID,
                    CreationDate = DateTime.Now,
                    Radius = 10,
                    Isapprove = false,
                    ADViews = 0,
                    NumberViews = 0,
                    IsNegotiable = model.IsNegotiable,
                    Email = model.Email,
                    Phone = model.Phone,
                    Longitude = model.Longitude,
                    Latitude = model.Latitude,
                    AccountID = account.ID,
                    IsDisabled = false,
                    Price = model.Price ?? 0
                };
                newAd.GenerateCode(_context);
                #endregion

                #region Ad category fields
                if (model.CategoryFieldOptions?.Count > 0)
                {
                    foreach (var item in model.CategoryFieldOptions)
                    {
                        var CatField = _context.Set<CategoryField>().SingleOrDefault(S => S.ID == item.CategoryFieldID);
                        CategoryFieldOption catOption = await _context.Set<CategoryFieldOption>().Include(x => x.CategoryField).FirstOrDefaultAsync(x => x.ID == item.CategoryFieldOptionID);
                        if (catOption == null) continue;
                        _context.Set<AdExtraField>().Add(new AdExtraField
                        {
                            Ad = newAd,
                            CategoryFieldOptionID = item.CategoryFieldOptionID,
                            Value = catOption?.Name,
                            Name = CatField?.Name
                        });
                    }
                }
                #endregion

                #region Upload Main Image
                if (model.MainImage?.Length > 0)
                {
                    string filename = ImageOperations.SaveImage(model.MainImage, newAd);
                    if (!string.IsNullOrEmpty(filename))
                    {
                        _context.Set<ADImage>().Add(new ADImage()
                        {
                            AD = newAd,
                            ImagePath = filename,
                            IsMain = true
                        });
                    }
                }
                #endregion

                #region Upload Other images
                if (model?.Images != null)
                {
                    foreach (var file in model.Images.Where(x => x.Length > 0))
                    {
                        string filename = ImageOperations.SaveImage(file, newAd);
                        if (!string.IsNullOrEmpty(filename))
                        {
                            _context.Set<ADImage>().Add(new ADImage()
                            {
                                AD = newAd,
                                ImagePath = filename,
                                IsMain = false
                            });
                        }
                    }
                }
                #endregion

                _context.Set<AD>().Add(newAd);
                await _context.SaveChangesAsync();
                return Ok(newAd.Code);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("{ID}")]
        [AuthorizeToken]
        public async Task<IActionResult> Delete(Guid ID)
        {
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _context.Set<Account>()
                .Include(x => x.ADs)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            var Ad = account.ADs.SingleOrDefault(x => x.ID == ID);
            if (Ad == null) return BadRequest("No ad related to this ID");

            Ad.IsDisabled = true;
            await _context.SubmitAsync();

            return Ok("Delete Successfuly");
        }
    }
}

