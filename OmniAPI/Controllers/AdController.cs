using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OmniAPI.Classes;
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
using SmartLifeLtd.Models;
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
        [AuthorizeToken]
        public override async Task<IActionResult> Get()
        {
            #region Check user
            Account account = null;
            ApplicationUser user = null;
            var userID = HttpContext.User.Identity.Name;
            if (userID != null)
            {
                user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                account = _context.Set<Account>()
                    .Include(x => x.Searches)
                    .FirstOrDefault(x => x.ID == user.AccountID);
            }
            #endregion

            var Items = await _context.Set<AD>()
                        .Include(x => x.ADImages)
                        .Include(x => x.Account)
                        .Include(x => x.Category)
                        .Include(x => x.Currency)
                        .Where(x => x.IsDisabled.GetValueOrDefault() != true)
                        .OrderByDescending(x => x.PublishedDate)
                        .ToListAsync();

            var returnedData = Items.Select(x => new ADDataModel
            {
                ID = x.ID,
                Name = x.Name,
                CategoryID = x.Category?.ID,
                Code = x.Code,
                PublishedDate = x.PublishedDate,
                Likes = x.FavouriteADs.Count,
                Views = x.ADViews,
                Price = x.Price,
                Title = x.Title,
                MainImage = x.GetMainImageRelativePath(),
                Category = x.Category?.Name,
                Currency = x.Currency?.Code ?? "SP",
                IsOwner = x.AccountID == account?.ID
            });
            return Json(returnedData);
        }

        [HttpGet]
        [AuthorizeToken]
        public async Task<IActionResult> GetPage([FromBody] PaginationDataModel model)
        {
            #region Check user
            Account account = null;
            ApplicationUser user = null;
            var userID = HttpContext.User.Identity.Name;
            if (userID != null)
            {
                user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                account = _context.Set<Account>()
                    .Include(x => x.Searches)
                    .FirstOrDefault(x => x.ID == user.AccountID);
            }
            #endregion

            try
            {
                var Items = await _context.Set<AD>()
                            .Include(x => x.ADImages)
                            .Include(x => x.Account)
                            .Include(x => x.Category)
                            .Include(x => x.Currency)
                            .Where(x => x.IsDisabled.GetValueOrDefault() != true)
                            .Skip(model.Page * model.Count).Take(model.Count).ToListAsync();
                switch (model.SortingType)
                {
                    case SortingType.HighToLowPrice:
                        Items = Items.OrderByDescending(x => x.Price).ToList();
                        break;
                    case SortingType.LowToHighPrice:
                        Items = Items.OrderBy(x => x.Price).ToList();
                        break;
                    case SortingType.MostRecently:
                    case SortingType.None:
                    default:
                        Items = Items.OrderByDescending(x => x.PublishedDate).ToList();
                        break;
                }
                var returnedData = Items.Select(x => new ADDataModel
                {
                    ID = x.ID,
                    Name = x.Name,
                    CategoryID = x.Category?.ID,
                    Code = x.Code,
                    PublishedDate = x.PublishedDate,
                    Likes = x.FavouriteADs.Count,
                    Views = x.ADViews,
                    Price = x.Price,
                    Title = x.Title,
                    MainImage = x.GetMainImageRelativePath(),
                    Category = x.Category?.Name,
                    Currency = x.Currency?.Code ?? "SP",
                    IsOwner = x.AccountID == account?.ID
                });
                return Json(returnedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
                         .Include(x => x.Currency)
                         .Include(x => x.ADImages)
                         .SingleOrDefault(c => c.ID == ID);
                if (Item == null)
                    return BadRequest("No Ad related to this ID");

                var jsonItem = new ADFullDataModel
                {
                    ID = Item.ID,
                    IsNegotiable = Item.IsNegotiable,
                    Views = Item.ADViews,
                    CategoryID = Item.CategoryID,
                    CategoryName = Item.Category?.Name,
                    Code = Item.Code,
                    CurrencyName = Item.Currency?.Code ?? "SP",
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
                    MainImage = Item.GetMainImageRelativePath(),
                    Images = Item.ADImages.Where(s => s.IsMain == false).Select(c => c.ImagePath).ToList(),
                    CategoryFieldOptions = Item.ADExtraFields.Select(a => new CategoryFieldOptionDataModel()
                    {
                        CategoryFieldID = a.CategoryFieldOption?.CategoryFieldID,
                        CategoryFieldOptionID = a.CategoryFieldOptionID,
                        Value = a.CategoryFieldOption?.Name
                    }).ToList()
                };
                return new JsonResult(jsonItem);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        //[AuthorizeToken]
        public async Task<IActionResult> Search([FromBody] SearchDataModel model)
        {
            #region Check user
            Account account = null;
            ApplicationUser user = null;
            var userID = HttpContext.User.Identity.Name;
            if (userID != null)
            {
                user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                account = _context.Set<Account>()
                    .Include(x => x.Searches)
                    .FirstOrDefault(x => x.ID == user.AccountID);
            }
            #endregion

            try
            {
                var Sort = EnumHelper<SortingType>.Parse(model.SortingType);
                var ads = _context.Set<AD>().Include(x => x.ADImages)
                    .Include(x => x.Currency)
                    .Include(x => x.Category)
                        .ThenInclude(x => x.Parent)
                            .ThenInclude(x => x.Parent)
                    .Where(x => x.IsDisabled.GetValueOrDefault() != true);

                if (!string.IsNullOrWhiteSpace(model.SearchWord))
                {
                    string searchWords = model.SearchWord.ToLower().Trim();
                    ads = ads.Where(x =>
                        x.Title.ToLower().Contains(searchWords) ||
                        x.Description.ToLower().Contains(searchWords) ||
                        x.Email.ToLower().Contains(searchWords) ||
                        (x.Category != null && x.Category.Name.ToLower().Contains(searchWords)) ||
                        x.Code.ToLower().Contains(searchWords) ||
                        x.Name.ToLower().Contains(searchWords) ||
                        x.Phone.ToLower().Contains(searchWords));
                }

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

                switch (Sort)
                {
                    case SortingType.None:
                    case SortingType.MostRecently:
                    default:
                        ads = ads.OrderByDescending(s => s.PublishedDate).Skip(model.Page * model.Count ?? 0).Take(model.Count ?? 10);
                        break;
                    case SortingType.HighToLowPrice:
                        ads = ads.OrderByDescending(s => s.Price).Skip(model.Page * model.Count ?? 0).Take(model.Count ?? 10);
                        break;
                    case SortingType.LowToHighPrice:
                        ads = ads.OrderBy(s => s.Price).Skip(model.Page * model.Count ?? 0).Take(model.Count ?? 10);
                        break;
                }

                var result = ads.ToList().Select(x => new ADDataModel
                {
                    ID = x.ID,
                    Name = x.Name,
                    Likes = x.NumberViews,
                    PublishedDate = x.PublishedDate,
                    Views = x.ADViews,
                    Price = x.Price,
                    Title = x.Title,
                    MainImage = x.GetMainImageRelativePath(),
                    CategoryID = x.CategoryID,
                    Code = x.Code,
                    Category = x.Category?.Name,
                    Currency = x.Currency?.Code ?? "SP",
                    IsOwner = x.AccountID == account?.ID
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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

            if (model == null || ModelState.IsValid == false) return BadRequest("Unvalid data model");

            var UpAd = _context.Set<AD>()
                .Include(x => x.Category)
                .Include(x => x.Account)
                .Include(x => x.ADExtraFields)
                .Include(x => x.ADImages)
                .SingleOrDefault(s => s.ID == ID);
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
                UpAd.CurrencyID = _context.Set<Currency>().FirstOrDefault(x => x.Code == model.CurrencyName)?.ID;

                _context.Set<AdExtraField>().RemoveRange(UpAd.ADExtraFields);
                _context.Set<ADImage>().RemoveRange(UpAd.ADImages);

                #region Update Category fields
                if (model.CategoryFieldOptions != null && model.CategoryFieldOptions.Count > 0)
                {
                    foreach (var item in model.CategoryFieldOptions)
                    {
                        var CatField = _context.Set<CategoryField>().Include(x => x.CategoryFieldOptions).SingleOrDefault(S => S.ID == item.CategoryFieldID);
                        CategoryFieldOption catOption = CatField.CategoryFieldOptions.FirstOrDefault(x => x.ID == item.CategoryFieldOptionID);
                        if (CatField == null) continue;
                        UpAd.ADExtraFields.Add(new AdExtraField
                        {
                            CategoryFieldOptionID = catOption?.ID,
                            Value = catOption?.Name ?? item.Value,
                            Name = CatField.Name
                        });
                    }
                }
                #endregion

                #region Update main images
                if (model.MainImage != null && model.MainImage.Length > 0)
                {
                    string filename = ImageOperations.SaveImage(model.MainImage, UpAd);
                    if (!string.IsNullOrWhiteSpace(filename))
                    {
                        UpAd.ADImages.Add(new ADImage()
                        {
                            ImagePath = filename,
                            IsMain = true
                        });
                    }
                }
                #endregion

                #region Save Other images
                if (model.Images != null && model.Images.Any(x => x.Length > 0))
                {
                    foreach (var file in model.Images.Where(x => x.Length > 0))
                    {
                        string filename = ImageOperations.SaveImage(file, UpAd);
                        if (!string.IsNullOrWhiteSpace(filename))
                        {
                            UpAd.ADImages.Add(new ADImage()
                            {
                                ImagePath = filename,
                                IsMain = false
                            });
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

            if (model == null || ModelState.IsValid == false) return BadRequest("Unvalid data model");

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
                    Price = (model.Price ?? 0) < 0 ? 0 : model.Price,
                    CurrencyID = _context.Set<Currency>().FirstOrDefault(x => x.Code == model.CurrencyName)?.ID,
                    PriceType = PriceType.Cash.ToString(),
                    ContactWay = ContactWay.Both.ToString(),
                    PublishedDate = DateTime.Now
                };
                bool codeGenerated = await newAd.GenerateCode(_context);
                if (codeGenerated == false) return BadRequest("Error in generate code");
                #endregion

                #region Ad category fields
                if (model.CategoryFieldOptions != null && model.CategoryFieldOptions.Count > 0)
                {
                    foreach (var item in model.CategoryFieldOptions)
                    {
                        var CatField = _context.Set<CategoryField>().Include(x => x.CategoryFieldOptions).SingleOrDefault(S => S.ID == item.CategoryFieldID);
                        CategoryFieldOption catOption = CatField.CategoryFieldOptions.FirstOrDefault(x => x.ID == item.CategoryFieldOptionID);
                        if (CatField == null) continue;
                        newAd.ADExtraFields.Add(new AdExtraField
                        {
                            CategoryFieldOptionID = catOption?.ID,
                            Value = catOption?.Name ?? item.Value,
                            Name = CatField.Name
                        });
                    }
                }
                #endregion

                #region Save Main Image
                if (model.MainImage != null && model.MainImage.Length > 0)
                {
                    string filename = ImageOperations.SaveImage(model.MainImage, newAd);
                    if (!string.IsNullOrWhiteSpace(filename))
                    {
                        newAd.ADImages.Add(new ADImage()
                        {
                            ImagePath = filename,
                            IsMain = true
                        });
                    }
                }
                #endregion

                #region Save Other images
                if (model.Images != null && model.Images.Any(x => x.Length > 0))
                {
                    foreach (var file in model.Images.Where(x => x.Length > 0))
                    {
                        string filename = ImageOperations.SaveImage(file, newAd);
                        if (!string.IsNullOrWhiteSpace(filename))
                        {
                            newAd.ADImages.Add(new ADImage()
                            {
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

            if (account.ADs == null) return BadRequest("Error in Get ads account");

            var Ad = account.ADs.SingleOrDefault(x => x.ID == ID);
            if (Ad == null) return BadRequest("No ad related to this ID");

            Ad.IsDisabled = true;
            await _context.SubmitAsync();

            return Ok("Delete Successfuly");
        }
    }
}

