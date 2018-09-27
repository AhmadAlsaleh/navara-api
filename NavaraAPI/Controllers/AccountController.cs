using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.ViewModels;
using SmartLifeLtd;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SmartLifeLtd.Management.Interfaces;
using SmartLifeLtd.ViewModels;

namespace NavaraAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private NavaraDbContext _Context { set; get; }
        public AccountController(NavaraDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _Context = context;
        }


        /// <summary>
        /// Gets the user informations that is saved in the database
        /// </summary>
        /// <returns></returns>
        [AuthorizeToken]
        public async Task<IActionResult> GetInformation()
        {
            try
            {
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _Context.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                if (user == null) return BadRequest("Error in get user or account data");
                Account account = _Context.Set<Account>().FirstOrDefault(x => x.ID == user.AccountID);
                if (account == null) return BadRequest("Error in get user or account data");
                return Json(new
                {
                    account.Name,
                    account.Mobile,
                    account.CartID,
                    account.CashBack,
                    account.LanguageID,
                    user.Email,
                    user.UserName,
                    IsVerified = user.PhoneNumberConfirmed || user.EmailConfirmed
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        [AuthorizeToken]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _Context.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                if (user == null) return BadRequest("Error in get user or account data");
                Account account = _Context.Set<Account>().FirstOrDefault(x => x.ID == user.AccountID);
                if (account == null) return BadRequest("Error in get user or account data");
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);

                var orders = _Context.Set<Order>().Include(x => x.OrderItems).Where(x => x.AccountID == account.ID);
                var json = new JsonResult(orders.Select(x => new OrderBasicModel()
                {
                    ID = x.ID,
                    Name = x.Name,
                    Date = x.CreationDate,
                    Code = x.Code,
                    TotalPrices = x.OrderItems.Sum(y => (y.UnitPrice ?? 0) * (y.Quantity ?? 1)),
                    TotalDiscount = x.OrderItems.Sum(y => y.UnitDiscount ?? 0),
                    NetTotalPrices = x.OrderItems.Sum(y => y.Total ?? 0),
                    Status = x.Status
                }));
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [AuthorizeToken]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _Context.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                if (user == null) return BadRequest("Error in get user or account data");
                Account account = _Context.Set<Account>()
                    .Include(x => x.Cart)
                        .ThenInclude(x => x.CartItems)
                            .ThenInclude(x => x.Offer)
                    .Include(x => x.Cart)
                        .ThenInclude(x => x.CartItems)
                            .ThenInclude(x => x.Item)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (account == null) return BadRequest("Error in get user or account data");
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);

                if (account.Cart == null)
                    return BadRequest("This account is not related to any cart");

                await account.Cart.UpdateCart(_Context);
                var json = new JsonResult(new CartViewModel()
                {
                    CreatedDate = account.Cart.CreationDate,
                    Items = account.Cart.CartItems.Select(x => new CartItemViewModel
                    {
                        ItemID = x.ItemID,
                        ItemName = x.Item?.Name,
                        ItemThumbnail = x.Item?.ThumbnailImagePath,
                        Quantity = x.Quantity,
                        UnitDiscount = x.UnitDiscount,
                        IsFree = x.IsFree,
                        OfferID = x.OfferID,
                        OfferTitle = x.Offer?.Title,
                        OfferThumbnail = x.Offer?.ThumbnailImagePath,
                        Total = x.Total,
                        UnitNetPrice = x.UnitNetPrice,
                        UnitPrice = x.UnitPrice
                    }).ToList()
                });
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Updates all the user information in the database
        /// </summary>
        /// <param name="userInfo">The object that holds the new information</param>
        /// <returns></returns>
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
                    ApplicationUser user = await _Context.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                    Account account = _Context.Set<Account>().FirstOrDefault(x => x.ID == user.AccountID);
                    if (user == null || account == null) return null;
                    account.Name = userInfo.FirstName;
                    if (user.UserName.Trim() != user.PhoneNumber.Trim())
                    {
                        account.Mobile = userInfo.PhoneNumber;
                        user.PhoneNumber = userInfo.PhoneNumber;
                    }
                    if (user.UserName.Trim() != user.Email.Trim())
                        user.Email = userInfo.Email;

                    await _Context.SubmitAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("Invaild information please check the sent information and try again");
        }
    }
}
