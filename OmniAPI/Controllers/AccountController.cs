using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using OmniAPI.Classes;
using OmniAPI.Controllers;
using OmniAPI.Models;
using SmartLifeLtd.API;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Omni;
using SmartLifeLtd.Data.Tables.Shared;
using SmartLifeLtd.IServices;
using SmartLifeLtd.ViewModels;
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
        private readonly IUsersService _usersService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory, IUsersService usersService,
            OmniDbContext context, LogDbContext logContext
           ) : base(context)
        {
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _userManager = userManager;
            _usersService = usersService;
        }

        [HttpGet]
        [AuthorizeToken]
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

            if (account.Searches == null)
                return BadRequest("Error in get saved searches");

            var searches = account.Searches.Select(x => new SavedSearchDataModel
            {
                Name = x.Name,
                SearchDate = x.SearchDate,
                FromPrice = x.FromPrice,
                ToPrice = x.ToPrice,
                SearchInDescription = x.SearchInDescription,
                CategoryID = x.CategoryID,
                Keywords = x.Keywords
            });
            return Ok(searches);
        }

        [HttpGet]
        [AuthorizeToken]
        public async Task<IActionResult> GetOwnItems()
        {
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _context.Set<Account>()
                .Include(x => x.ADs)
                    .ThenInclude(x => x.ADImages)
                .Include(x => x.ADs)
                    .ThenInclude(x => x.Currency)
                .Include(x => x.ADs)
                    .ThenInclude(x => x.Category)
                .Include(x => x.ADs)
                    .ThenInclude(x => x.FavouriteADs)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            if (account.ADs == null) return BadRequest("Error in get Ads account");

            List<ADDataModel> Data = new List<ADDataModel>();
            foreach (var ad in account.ADs)
            {
                if (ad == null || ad.IsDisabled.GetValueOrDefault() == true) continue;
                try
                {
                    var item = new ADDataModel()
                    {
                        ID = ad.ID,
                        Name = ad.Name,
                        Title = ad.Title,
                        Currency = ad.Currency?.Code ?? "SP",
                        Price = ad.Price,
                        Code = ad.Code,
                        Views = ad.ADViews,
                        CategoryID = ad.CategoryID,
                        Likes = ad.FavouriteADs.Count(),
                        Category = ad.Category?.Name,
                        PublishedDate = ad.PublishedDate,
                        MainImage = ad.GetMainImageRelativePath()
                    };
                    Data.Add(item);
                }
                catch { }
            }
            return Json(Data);
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

            if (account.Chats == null) return BadRequest("Error in get chats account");

            var chats = account.Chats.Where(x => x?.AD != null).Select(chat => new ChatDataModel()
            {
                ChatID = chat.ID,
                AdID = chat.ADID,
                Owner = chat.OwnerName,
                OwnerAccountID = chat.OwnerAccountID,
                AdTitle = chat.AD.Title,
                Image = chat.AD.GetMainImageRelativePath(),
                Messages = new List<ChatMessageDataModel>() {
                    chat.Messages?.OrderByDescending(x => x.SentDate).Select(msg => new ChatMessageDataModel()
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
            if (string.IsNullOrEmpty(account.ImagePath) || !System.IO.File.Exists(account.ImagePath?.GetFilePathOnServer()))
                image = $"images/defaultPerson.gif";
            var accountInfo = new
            {
                image,
                account.Name,
                account.Latitude,
                account.Longitude,
                user.PhoneNumber,
                user.Email,
                user.UserName,
                IsVerified = user.PhoneNumberConfirmed || user.EmailConfirmed,
                account.LanguageID
            };
            return Ok(accountInfo);
        }

        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> UpdateInformation([FromBody]UpdateUserInformationDataModel userInfo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userID = HttpContext.User.Identity.Name;
                    if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                    ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                    Account account = _context.Set<Account>().FirstOrDefault(x => x.ID == user.AccountID);
                    if (user == null || account == null) return null;

                    var result = await _usersService.UpdateUserInformation(userID, userInfo);
                    if ((bool?)result == true)
                        return Ok();
                    else return BadRequest("Faild in save data");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("Invaild information please check the sent information and try again");
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

            if (account.FavouriteADs == null) return BadRequest("Error in get favorite ads");
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
                        .ThenInclude(x => x.Currency)
                .Include(x => x.FavouriteADs)
                    .ThenInclude(x => x.AD)
                        .ThenInclude(x => x.ADImages)
                .Include(x => x.FavouriteADs)
                    .ThenInclude(x => x.AD)
                        .ThenInclude(x => x.FavouriteADs)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            if (account.FavouriteADs == null) return BadRequest("Error in get favorite ads");

            List<ADDataModel> Data = new List<ADDataModel>();
            foreach (var favAd in account.FavouriteADs)
            {
                if (favAd.AD == null || favAd.AD.IsDisabled.GetValueOrDefault() == true) continue;
                var item = new ADDataModel()
                {
                    ID = favAd.AD.ID,
                    Name = favAd.AD.Name,
                    Title = favAd.AD.Title,
                    Currency = favAd.AD.Currency?.Code ?? "SP",
                    Price = favAd.AD.Price,
                    Code = favAd.AD.Code,
                    Views = favAd.AD.ADViews,
                    Likes = favAd.AD.FavouriteADs.Count(),
                    Category = favAd.AD.Category?.Name,
                    CategoryID = favAd.AD.CategoryID,
                    PublishedDate = favAd.AD.PublishedDate,
                    MainImage = favAd.AD.GetMainImageRelativePath()
                };

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
                    await _context.SubmitAsync();
                    return Ok("AD Added from favorite");
                }
                else
                {
                    _context.Set<SavedAD>().Remove(favAd);
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
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _context.Set<Account>()
                .Include(x => x.Searches)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            if (model == null || ModelState.IsValid == false) return BadRequest("Unvalid recieved data");

            try
            {
                Search NewSearch = new Search
                {
                    AccountID = account.ID,
                    Keywords = model.SearchWord,
                    CreationDate = DateTime.Now,
                    CategoryID = model.CategoryID,
                    FromPrice = model.LowPrice,
                    ToPrice = model.HighPrice,
                    SearchDate = DateTime.Now,
                    SearchInDescription = true,
                    OnlyWithPhoto = false,
                    Name = "Search " + DateTime.Now.ToString("ddMMyyyyHHmmss")
                };
                _context.Set<Search>().Add(NewSearch);
                await _context.SubmitAsync();
                return Ok("Saved search criteria successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
