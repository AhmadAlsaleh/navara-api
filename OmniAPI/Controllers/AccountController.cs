using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using OmniAPI.Controllers;
using OmniAPI.Models;
using SmartLifeLtd.API;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Omni;
using SmartLifeLtd.Data.Tables.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Omni.Controllers.API
{
    [Route("[controller]/[Action]")]
    public class AccountController : BaseController<Account>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;


        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory,
            OmniDbContext context, LogDbContext logContext
           ) : base(context)
        {
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _userManager = userManager;
        }

        [AuthorizeToken]
        [HttpGet]
        public async Task<IActionResult> GetSearches()
        {
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _context.Set<Account>()
                .Include(x => x.Searches)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            return Ok(account.Searches.Select(x => new
            {
                x.Name,
                x.SearchDate,
                x.FromPrice,
                x.ToPrice,
                x.SearchInDescription,
                x.CategoryID,
                x.Keywords
            }));
        }

        [AuthorizeToken]
        [HttpGet]
        public async Task<IActionResult> GetOwnItems()
        {
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _context.Set<Account>()
                .Include(x => x.ADs)
                    .ThenInclude(x => x.ADImages)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            string DefCurrency = _context.Set<Currency>().FirstOrDefault(x => x.IsDefault == true)?.Symbol ?? "";
            List<ADDataModel> Data = new List<ADDataModel>();
            foreach (var ad in account.ADs)
            {
                if (ad.IsDisabled == false) continue;
                var item = new ADDataModel()
                {
                    ID = ad.ID,
                    Name = ad.Name,
                    Title = ad.Title,
                    Currency = ad?.Currency?.Symbol ?? DefCurrency,
                    Price = ad.Price,
                    Code = ad.Code,
                    Views = ad.ADViews,
                    CategoryID = ad.CategoryID,
                    Likes = _context.Set<SavedAD>().Count(x => x.ADID == ad.ID),
                    Category = ad.Category?.Name,
                    PublishedDate = ad.PublishedDate
                };
                ADImage adImage = ad.ADImages.FirstOrDefault(x => x.IsMain == true);
                string filePath = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                if (adImage != null)
                {
                    filePath = $"{this.HttpContext.Request.Host.Value}/{adImage.ImagePath.Replace("\\", "/")}";
                    string physicalFilePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\{adImage.ImagePath}";
                    if (!System.IO.File.Exists(physicalFilePath))
                        filePath = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                }
                item.MainImage = filePath;
                Data.Add(item);
            }
            return Json(Data);
        }

        [AuthorizeToken]
        [HttpGet]
        public async Task<IActionResult> GetChatNotSeen()
        {
            try
            {
                #region Check user
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = _context.Set<Account>()
                    .Include(x => x.Chats)
                        .ThenInclude(x => x.AD)
                    .Include(x => x.Chats)
                        .ThenInclude(x => x.Messages)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                #endregion

                var allChats = account.Chats.Where(x => x.Messages.Any(y => y.SeenDate == null));
                return Ok(allChats);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [AuthorizeToken]
        public async Task<IActionResult> GetChats()
        {
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _context.Set<Account>()
                .Include(x => x.Chats)
                    .ThenInclude(x => x.AD)
                        .ThenInclude(x => x.ADImages)
                .Include(x => x.Chats)
                    .ThenInclude(x => x.Messages)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            var chats = account.Chats.Where(x => x.AD != null).Select(chat => new ChatDataModel()
            {
                ChatID = chat.ID,
                AdID = chat.ADID,
                Owner = chat.OwnerName,
                OwnerAccountID = chat.OwnerAccountID,
                AdTitle = chat.AD.Title,
                Image = chat.AD.ADImages.SingleOrDefault(S => S.IsMain == true)?.ImagePath ?? "",
                Messages = new List<ChatMessageDataModel>() {
                    chat.Messages.OrderByDescending(x => x.SentDate).Select(msg => new ChatMessageDataModel()
                    {
                        Body = msg.Body,
                        SeenDate = msg.SeenDate,
                        ReceiverID = msg.ReceiverAccountID,
                        SenderName = msg.SenderName,
                        SentDate = msg.SentDate,
                        Title = msg.Title
                    }).FirstOrDefault()
                }
            }).ToList();
            return Json(chats);
        }

        [HttpGet]
        [AuthorizeToken]
        public async Task<IActionResult> GetAccountInfo()
        {
            try
            {
                #region Check user
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = _context.Set<Account>()
                    .Include(x => x.ADs)
                        .ThenInclude(x => x.ADImages)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                #endregion

                var image = account.ImagePath;
                if (string.IsNullOrEmpty(account.ImagePath))
                    image = $"/images/defaultPerson.gif";
                return Ok(new
                {
                    image = account.ImagePath,
                    Phone = account.Phone,
                    Name = account.Name
                });
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        #region Favorite
        [AuthorizeToken]
        [HttpGet("{ID}")]
        public async Task<IActionResult> IsFavourite(Guid ID)
        {
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _context.Set<Account>()
                .Include(x => x.FavouriteADs)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            return Ok(account.FavouriteADs.Any(x => x.ADID == ID));
        }

        [AuthorizeToken]
        [HttpGet]
        public async Task<IActionResult> GetFavoriteItems()
        {
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _context.Set<Account>()
                .Include(x => x.FavouriteADs)
                    .ThenInclude(x => x.AD)
                        .ThenInclude(x => x.Category)
                .Include(x => x.FavouriteADs)
                    .ThenInclude(x => x.AD)
                        .ThenInclude(x => x.ADImages)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            List<ADDataModel> Data = new List<ADDataModel>();
            foreach (var favAd in account.FavouriteADs)
            {
                var item = new ADDataModel()
                {
                    ID = favAd.AD.ID,
                    Name = favAd.AD.Name,
                    Title = favAd.AD.Title,
                    Currency = favAd.AD.Currency?.Name,
                    Price = favAd.AD.Price,
                    Code = favAd.AD.Code,
                    Views = favAd.AD.ADViews,
                    Likes = _context.Set<SavedAD>().Count(x => x.ADID == favAd.ADID),
                    Category = favAd.AD.Category.Name,
                    CategoryID = favAd.AD.CategoryID,
                    PublishedDate = favAd.AD.PublishedDate
                };

                ADImage adImage = favAd.AD.ADImages.FirstOrDefault(x => x.IsMain == true);
                string filePath = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                if (adImage != null)
                    filePath = adImage.ImagePath;
                item.MainImage = filePath;
                Data.Add(item);
            }
            return Json(Data);
        }

        [HttpPost("{ID}")]
        [AuthorizeToken]
        public async Task<IActionResult> AddToFavorite(Guid ID)
        {
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _context.Set<Account>()
                .Include(x => x.ADs)
                    .ThenInclude(x => x.ADImages)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            try
            {
                var favAd = _context.Set<SavedAD>().SingleOrDefault(x => x.AccountID == account.ID && x.ADID == ID);
                if (favAd == null)
                {
                    _context.Set<SavedAD>().Add(new SavedAD()
                    {
                        ADID = ID,
                        Account = account,
                        AddedDate = DateTime.Now,
                        CreationDate = DateTime.Now
                    });
                    var AD = _context?.Set<AD>().SingleOrDefault(s => s.ID == ID);
                    AD.NumberViews++;
                    await _context.SubmitAsync();
                    return Ok("AD Added from favorite");
                }
                else
                {
                    _context.Set<SavedAD>().Remove(favAd);
                    var AD = _context?.Set<AD>()?.SingleOrDefault(s => s.ID == ID);
                    AD.NumberViews--;
                    await _context.SubmitAsync();
                    return Ok("AD Removed from favorite");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        [HttpPost]
        [AuthorizeToken]
        public async Task<IActionResult> SaveSearch([FromBody] SearchDataModel model)
        {
            try
            {
                #region Check user
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = _context.Set<Account>()
                    .Include(x => x.Searches)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                #endregion

                Search NewSearch = new Search
                {
                    AccountID = account.ID,
                    Keywords = model.SearchWord,
                    CreationDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    CategoryID = model.CategoryID,
                    FromPrice = (int?)model.LowPrice,
                    ToPrice = (int?)model.HighPrice,
                    SearchDate = DateTime.Now,
                    Name = account.Name + DateTime.Now
                };
                _context.Set<Search>().Add(NewSearch);
                await _context.SubmitAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
    }
}
