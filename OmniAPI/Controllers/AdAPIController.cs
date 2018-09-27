using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OmniAPI.Models;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Omni;
using SmartLifeLtd.Data.Tables.OMNI;
using SmartLifeLtd.Enums;
using SmartLifeLtd.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Omni.Controllers.API
{
    [Route("api/[controller]/[Action]")]
    public class AdAPIController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OmniDbContext _context;

        public AdAPIController(
            UserManager<ApplicationUser> userManager
            , OmniDbContext context
           )
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("{Token}")]
        public async Task<IActionResult> GetOwnAds(Guid Token)
        {
            var account = _context.Accounts.Include("AccountTokens").Include("FavouriteADs")
                .Where(x => x.AccountTokens.Any(y => y.Token == Token.ToString()))
                .SingleOrDefault();
            if (account == null)
                return BadRequest("Account Token is not relatred to any account");
            string DefCurrency = _context.Currencies.FirstOrDefault(x => x.IsDefault == true)?.Symbol ?? "";

            List<AccountFavouriteViewModel> Data = new List<AccountFavouriteViewModel>();
            foreach (var favAd in account.ADs)
            {
                var orginal = _context.ADs.Include("ADImages").SingleOrDefault(x => x.ID == favAd.ID&& x.IsDisabled == false) ;
                if (orginal == null) continue;
                int favCount = _context.SavedADs.Count(x => x.ADID == orginal.ID);
                var item = new AccountFavouriteViewModel()
                {
                    ID = orginal.ID,
                    Name = orginal.Name,
                    Title = orginal.Title,
                    Currency = orginal?.Currency?.Symbol ?? DefCurrency,
                    Price = orginal.Price,
                    Code = orginal.Code,
                    ViewsNum = orginal.ADViews,
                    IsNegotiable=orginal.IsNegotiable,
                    FavNum = favCount
                };
                ADImage adImage = favAd.ADImages.FirstOrDefault(x => x.IsMain == true);
                string filePath = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                if (adImage != null)
                {
                    filePath = $"{this.HttpContext.Request.Host.Value}/{adImage.ImagePath.Replace("\\", "/")}";
                    string physicalFilePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\{adImage.ImagePath}";
                    if (!System.IO.File.Exists(physicalFilePath))
                        filePath = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                }
                item.Image = filePath;
                Data.Add(item);
            }
            return Json(Data);
        }
        [HttpGet("{ID}")]
        public IActionResult GetAdInfo(Guid ID)
        {
            try
            {
                double RemoveNull = new double();
                var Items = _context.ADs.Include("ADExtraFields").Include("ADExtraFields.CategoryField").Include("ADImages").Include("Account")?.Include("Category")
                   .Include("Currency").Include("Area").Include("Area.City").Include("Area.City.Country")
                .Select(x => new
                {
                    ID = x.ID,
                    x.IsNegotiable,
                    Views = x.ADViews,
                    //Tarek AreaName = x.Area.Name,
                    categoryID = x.CategoryID,
                    CategoryName = x.Category.Name,
                    //Tarek AreaID = x.AreaID,
                    //Tarek CityID = x.Area.CityID,
                    //Tarek CountryName = x.City.Country.Name ?? "",
                    //Tarek CityName = x.Area.City.Name ?? "",
                    Code = x.Code,
                    CurrencyName = x.Currency.Name,
                    Description = x.Description,
                    Name = x.Name,
                    Phone = x.Phone,
                    Currency = x.Currency.Code ?? "sp",
                    Hearts = x.NumberViews,
                    Price = x.Price,
                    //Tarek  Longitude = x.Area.longitude ?? RemoveNull,
                    //Tarek   latitude = x.Area.latitude ?? RemoveNull,
                    PublishedDate = x.PublishedDate.Year + "/" + x.PublishedDate.Month + "/" + x.PublishedDate.Day,
                    Title = x.Title,
                    UpdatedDate = x.UpdatedDate,
                    UserID = x.AccountID,
                    UserName = x.Account.Name,
                    MainImage = x.ADImages.FirstOrDefault(y => y.IsMain == true).ImagePath,
                    Images = x.ADImages.Where(s=>s.IsMain==false).Select(c => c.ImagePath).ToList(),
                    CategoryFields = x.ADExtraFields.Select(a => new
                    {
                        //Tarek
                        CategoryFieldID = a.CategoryFieldOptionID,
                       //Tarek Type = a.CategoryField.Type,
                        Name = a.CategoryFieldOption.Name,
                        Value = a.Value
                    }),
                }).SingleOrDefault(c => c.ID == ID);
                var json = new JsonResult(Items);
                return json;
            }
            catch (Exception ex)
            {

                return BadRequest();
            }
           
        }
        [HttpGet("{AccountID}")]
        public async Task<IActionResult> GetAccountAds(Guid AccountID)
        {
            try
            {
                var account = _context.Accounts.Include("AccountTokens").Include("FavouriteADs")
             .Where(x => x.ID == AccountID)
             .SingleOrDefault();
                if (account == null)
                    return BadRequest("Account ID is not relatred to any account");
                List<AccountItemViewModel> Data = new List<AccountItemViewModel>();
                foreach (var favAd in account.ADs)
                {
                    var orginal = _context.ADs.Include("Currency").Include("ADImages").SingleOrDefault(x => x.ID == favAd.ID && x.IsDisabled == false);
                    if (orginal == null) continue;
                    int favCount = _context.SavedADs.Count(x => x.ADID == orginal.ID);
                    var item = new AccountItemViewModel()
                    {
                        ID = orginal.ID,
                        Name = orginal.Name,
                        IsNegotiable=orginal.IsNegotiable,
                        Title = orginal.Title,
                        CurrencyName = orginal.Currency?.Name,
                        Price = orginal.Price,
                        Code = orginal.Code,
                        ViewsNum = orginal.ADViews,
                        FavNum = favCount,
                        MainImage = orginal.ADImages?.SingleOrDefault(s => s.IsMain == true).ImagePath,
                        PublishedDate=orginal.CreationDate
                    };
                    ADImage adImage = favAd.ADImages.FirstOrDefault(x => x.IsMain == true);
                    string filePath = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                    if (adImage == null)
                    {

                        item.MainImage = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                    }

                    Data.Add(item);
                }
                return Json(Data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
             
            }
         
        }


        [HttpGet("{Token}")]
        public async Task<IActionResult> GetFavouriteAds(Guid Token)
        {
            var account = _context.Accounts.Include("AccountTokens").Include("FavouriteADs")?.Include("FavouriteADs.Ad")
                .Where(x => x.AccountTokens.Any(y => y.Token == Token.ToString()))
                .SingleOrDefault();
            if (account == null)
                return BadRequest("Account Token is not relatred to any account");
            List<AccountFavouriteViewModel> Data = new List<AccountFavouriteViewModel>();
            foreach (var favAd in account.FavouriteADs.Where(x=>x.AD.IsDisabled==false))
            {
                var orginal = _context.ADs.Include("ADImages").SingleOrDefault(x => x.ID == favAd.ADID && x.IsDisabled == false);
                int favCount = _context.SavedADs.Count(x => x.ADID == orginal.ID);

                if (orginal == null) continue;
                var item = new AccountFavouriteViewModel()
                {
                    ID = orginal.ID,
                    Name = orginal.Name,
                    Title = orginal.Title,
                    Currency = orginal.Currency?.Name,
                    Price = orginal.Price,
                    Code = orginal.Code,
                    ViewsNum = orginal.ADViews,
                    FavNum = favCount,
                    IsNegotiable=orginal.IsNegotiable
                };
               
                ADImage adImage = orginal.ADImages.FirstOrDefault(x => x.IsMain == true);
                string filePath = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                if (adImage != null)
                {
                    filePath = adImage.ImagePath;
                    //filePath = $"{this.HttpContext.Request.Host.Value}/images/ItemImages/{orginal.Code}/{adImage.ImagePath}";
                    //if (!System.IO.File.Exists(filePath))
                    //    filePath = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                }
                item.Image = filePath;
                Data.Add(item);
            }
            return Json(Data);
        }

        [HttpPost]
        public async Task<IActionResult> GetAds([FromBody] AdPagesViewModel model)
        {
            var Sort = EnumHelper<SortingType>.Parse(model.SortingType);
            var ads = _context.ADs.Include("ADImages").Include("Account")?.Include("Category").Include("Category.Parent")
                       .Include("Category.Parent.Parent")
                   .Include("Currency").Include("Area").Include("Area.City")
                   .Where(x => model.SearchWord == null || x.Title.Contains(model.SearchWord))
                   .Where(x=>model.HighPrice==null&&model.LowPrice==null ||x.Price>=model.LowPrice&&x.Price<=model.HighPrice)
                   /*Tarek .Where(x=>model.AreaID==null||model.AreaID == Guid.Emptyx.AreaID)*/
                   .Where(x=>x.IsDisabled==false);


            if (model.CategoryID != null)
            {

                ads = ads.Where(x => (x.CategoryID == model.CategoryID) ||
                  (x.Category != null && x.Category.ParentID == model.CategoryID) ||
                  (x.Category != null && x.Category.Parent != null && x.Category.Parent.ParentID == model.CategoryID));

            }
            string DefCurrency = _context.Currencies.FirstOrDefault(x => x.IsDefault == true)?.Symbol ?? "";

            //var  adslist = ads.ToList();
            var result = _context.ADs.Include("ADImages")?
               .Include("Currency").Include("Area").Include("Area.City")?.Where(p => ads.Any(p2 => p2.ID == p.ID))
               .Select(x => new GetAdViewModel
               {
                   ID = x.ID,
                   IsNegotiable=x.IsNegotiable,
                   CategoryID = x.Category.ID,
                   //Tarek  CityName = x.City.Name,
                   //Tarek  AreaID = x.AreaID,
                   CurrencyName = (x.Currency == null ? null : x.Currency.Symbol) ?? DefCurrency,
                   Name = x.Name,
                   Hearts = x.NumberViews,
                   PublishedDate = x.PublishedDate,
                   View = x.ADViews,
                   Price = x.Price,
                   Title = x.Title,
                   IsDisabled = x.IsDisabled,
                   MainImage = x.ADImages.FirstOrDefault(y => y.IsMain == true).ImagePath
               });
            try
            {
                List<GetAdViewModel> FinalResult = new List<GetAdViewModel>();
                switch (Sort)
                {
                    case SortingType.None: FinalResult = result.OrderByDescending(s => s.PublishedDate).Skip(model.Page * model.Count).Take(model.Count).ToList(); break;
                    case SortingType.MostRecently:
                        FinalResult=result.OrderByDescending(s => s.PublishedDate).Skip(model.Page * model.Count).Take(model.Count).ToList(); break;
                    case SortingType.HighToLowPrice: FinalResult= result.OrderByDescending(s => s.Price).Skip(model.Page * model.Count).Take(model.Count).ToList(); break;
                    case SortingType.LowToHighPrice: FinalResult= result.OrderBy(s => s.Price).Skip(model.Page * model.Count).Take(model.Count).ToList(); break;
                }
                return Ok(new { ADs = FinalResult });
            }
            catch (Exception e)
            {
                string s = e.Message;
                string[] arr = new string[0];
                return Ok(new { ADs = arr });
            }


        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddToFavorite([FromBody]AddToFavoriteViewModel model)
        {
            if (model.AdID == null) return BadRequest();


            try
            {
                var User = _context?.Accounts?.SingleOrDefault(x => x.ID == model.UserID);
                if (User != null)
                {

                    var favAd = _context.SavedADs.SingleOrDefault(x => x.AccountID == model.UserID && x.ADID == model.AdID);
                    if (favAd == null)
                    {
                        _context.SavedADs.Add(new SavedAD() { ADID = model.AdID, Account = User, AddedDate = DateTime.Now,CreationDate=DateTime.Now });
                        var AD = _context?.ADs?.SingleOrDefault(s => s.ID == model.AdID);
                        AD.NumberViews++;
                        _context.SubmitAsync();
                        //  return View();
                        // return Content("<span id='star' class='fa fa-star'></span>");
                        return Ok("Saved Ad");

                    }
                    else
                    {
                        _context.SavedADs.Remove(favAd);
                        var AD = _context?.ADs?.SingleOrDefault(s => s.ID == model.AdID);
                        AD.NumberViews--;
                        _context.SubmitAsync();
                        // return View();
                        //   return Content("<span id='star' class='fa fa-star-o'></span>");
                        return Ok("Removed Ad");
                    }
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return BadRequest();
        }


        [HttpPost]
        public IActionResult DeleteAd([FromBody]DeleteAdViewModel model)
        {
            var Ad = _context?.ADs?.SingleOrDefault(x => x.ID == model.AdID);
            if (Ad == null) return BadRequest("Ad is not exist");

            try
            {
                Ad.IsDisabled = true;
                _context.SubmitAsync();
                return Ok("Delete Successfuly");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult TestRemove()
        {
            String path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", @"images\ItemImages\0001211112\3d56b41f-c926-463e-8d7f-341c3e21d6d9.png");
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                return Ok();
            }
            return BadRequest();
        }
        public string SaveImage(string ImgStr, AD newAd)
        {
            String path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images\\ItemImages\\" + newAd.Code);

            //Check if directory exist
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }

            string imageName = Guid.NewGuid() + ".jpg";

            //set the image path
            string imgPath = Path.Combine(path, imageName);

            byte[] imageBytes = Convert.FromBase64String(ImgStr);

            System.IO.File.WriteAllBytes(imgPath, imageBytes);
            return imageName;
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult IsFavouriteAd([FromBody] IsFavouriteViewModel model)
        {
            try
            {
                var IsFav = _context?.SavedADs?.SingleOrDefault(x => x.AccountID == model.UserID && x.ADID == model.AdID);
                if (IsFav != null)
                {
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateAd([FromBody] UpdateAdViewModel model)
        {

         var   UpAd = _context?.ADs?.Include("Area")?.Include("Area.City")?.Include("Category")?.Include("Account")?.SingleOrDefault(s => s.ID == model.AdID);
            if (UpAd != null)
            {
                try
                {
                    Guid catId = new Guid();
                    if (model.CategoryLevel1ID != null)
                        catId = (Guid)model.CategoryLevel1ID;
                    if (model.CategoryLevel2ID != null)
                        catId = (Guid)model.CategoryLevel2ID;
                    if (model.CategoryLevel3ID != null)
                        catId = (Guid)model.CategoryLevel3ID;

                    //Tarek var Area = _context?.Areas?.SingleOrDefault(s => s.ID == model.AreaID);
                    var category = _context?.Categories?.SingleOrDefault(s => s.ID == catId);
                    UpAd.Category = category;
                    //Tarek  UpAd.Area = Area;
                    UpAd.Name = model.Name;
                    UpAd.Title = model.Title;
                    UpAd.Description = model.Description;
                    UpAd.Email = model.Email;
                    UpAd.Phone = model.Phone;
                    UpAd.IsDisabled = false;
                    UpAd.Price = Double.Parse(model.Price ?? "0");
                 
                    if (model?.DeletedImages?.Count > 0)
                    {
                        foreach (var item in model.DeletedImages)
                        {
                            var image = _context?.ADImages?.SingleOrDefault(s => s.ImagePath == item);
                            String path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImagePath);
                            if (System.IO.File.Exists(path))
                            {
                                _context.ADImages.Remove(image);
                                System.IO.File.Delete(path);

                            }
                        }
                    }
                    var OldField = _context?.ADExtraFields?.Where(s => s.AdID == UpAd.ID).ToList();
                    if (OldField.Count > 0)
                    {
                        foreach (var item in OldField)
                        {
                            _context.ADExtraFields.Remove(item);
                        }
                    }
                    if (model.CategoryFields?.Count > 0)
                    {
                        foreach (var item in model.CategoryFields)
                        {
                            var CatField = _context?.CategoryFields?.SingleOrDefault(S => S.ID == item.CategoryFieldID);
                            _context.ADExtraFields.Add(new AdExtraField {
                                Ad = UpAd,
                                CategoryFieldOptionID = item.CategoryFieldID,
                                Value = item.value,
                                Name= CatField.Name,
                                Type= CatField.Type
                            });

                        }
                    }

                    var uploadpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images\\ItemImages\\" + UpAd.Code);
                    if (!string.IsNullOrEmpty(model.MainImage))
                    {
                        var image = _context?.ADImages?.SingleOrDefault(s => s.ImagePath == model.MainImage);
                        String path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImagePath);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);

                        }
                            UpAd.ADImages.Remove(image);
                            #region Upload Main Image
                            if (model.MainImage?.Length > 0)
                        {
                            string filename = SaveImage(model.MainImage, UpAd);
                            if (!string.IsNullOrEmpty(filename))
                            {
                                ADImage pic = new ADImage()
                                {
                                    AD = UpAd,
                                    ImagePath = "images\\ItemImages\\" + UpAd.Code + "\\" + filename,
                                    IsMain = true
                                };
                                    _context.ADImages.Add(pic);
          
                            }

                        }

                        #endregion
                    }




                    #region Upload Other images
                    if (model?.Photos != null)
                    {
                        foreach (var file in model?.Photos)
                        {
                            if (file.Length > 0)
                            {

                                string filename = SaveImage(file, UpAd);
                                if (!string.IsNullOrEmpty(filename))
                                {
                                    ADImage pic = new ADImage()
                                    {
                                        AD = UpAd,
                                        ImagePath = "images\\ItemImages\\" + UpAd.Code + "\\" + filename,
                                        IsMain = false
                                    };
                                    _context.ADImages.Add(pic);
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
            else
            {
                return BadRequest("No Ad");
            }

        }
        [HttpPost]
        [AllowAnonymous]
        public async Task< IActionResult> NewAdd([FromBody] NewAdViewModel model)
        {
            var Account = _context.Accounts?.SingleOrDefault(X => X.ID == model.UserID);
            if (Account != null) {
                try
                {
                    Guid catId = new Guid();
                    if (model.CategoryLevel1ID != null)
                        catId = (Guid)model.CategoryLevel1ID;
                    if (model.CategoryLevel2ID != null)
                        catId = (Guid)model.CategoryLevel2ID;
                    if (model.CategoryLevel3ID != null)
                        catId = (Guid)model.CategoryLevel3ID;

                    AD newAd = new AD()
                    {
                        CategoryID = catId,
                        CreationDate = DateTime.Now,
                        PublishedDate = DateTime.Now,
                        ADViews = 0,
                        NumberViews = 0,
                        Name = model.Name,
                        Title = model.Title,
                        Description = model.Description,
                        IsNegotiable = true,
                        Email = model.Email,
                        Phone = model.Phone,
                        //Tarek   CityID = model.CityID,
                        //Tarek  AreaID = model.AreaID,
                        Account = Account,
                        IsDisabled = false,

                        Price = Double.Parse(model.Price ?? "0")
                    };
                    if (model.CategoryFields?.Count > 0)
                    {
                        foreach (var item in model.CategoryFields)
                        {
                            var CatField = _context?.CategoryFields?.SingleOrDefault(S => S.ID == item.CategoryFieldID);
                            _context.ADExtraFields.Add(new AdExtraField
                            {
                                Ad = newAd,
                                Name = CatField.Name,
                                Type = CatField.Type,
                                CategoryFieldOptionID =item.CategoryFieldID,
                                Value =item.value 
                            });

                        }
                    }
                    newAd.GenerateCode(_context);

                
                    if (newAd.CategoryID == Guid.Empty) throw new Exception();
                    var uploadpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images\\ItemImages\\" + newAd.Code);
                    Directory.CreateDirectory(uploadpath);
                    #region Upload Main Image
                    if (model.MainImage?.Length > 0)
                    {
                       string filename= SaveImage(model.MainImage, newAd);
                        if (!string.IsNullOrEmpty(filename))
                        {
                            ADImage pic = new ADImage()
                            {
                                AD = newAd,
                                ImagePath = "images\\ItemImages\\" + newAd.Code+"\\"+filename,
                                IsMain = true
                            };
                            _context.ADImages.Add(pic);
                        }
                 
                        }

                    #endregion
                    #region Upload Other images
                    if (model?.Photos != null)
                    {
                        foreach (var file in model?.Photos)
                        {
                            if (file.Length > 0)
                            {

                                string filename = SaveImage(file, newAd);
                                if (!string.IsNullOrEmpty(filename))
                                {
                                    ADImage pic = new ADImage()
                                    {
                                        AD = newAd,
                                        ImagePath = "images\\ItemImages\\" + newAd.Code + "\\" + filename,
                                        IsMain = false
                                    };
                                    _context.ADImages.Add(pic);
                                }
                                }
                            }
                        }

                    #endregion
                    _context.ADs.Add(newAd);
                 await  _context.SaveChangesAsync();
                
                    return Ok(newAd.ID);
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            else
            {
                return BadRequest("No Account User");
            }
      
        }
    }
}

