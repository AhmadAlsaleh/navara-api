using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.Models;
using NavaraAPI.Services;
using NavaraAPI.ViewModels;
using SmartLifeLtd;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Enums;
using NavaraAPI.IServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
                Account account = _Context.Set<Account>().FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
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
                Account account = _Context.Set<Account>().FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);

                var orders = _Context.Set<Order>().Include(x => x.OrderItems).Where(x => x.AccountID == account.ID);
                var json = new JsonResult(orders.Select(x => new OrderBasicModel()
                {
                    ID = x.ID,
                    Name = x.Name,
                    Date = x.CreationDate,
                    Code = "ORD009101FA1"
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
                Account account = _Context.Set<Account>()
                    .Include(x => x.Cart)
                        .ThenInclude(x => x.CartItems)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return BadRequest("Error in get user or account data");
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);

                if (account.Cart == null)
                    return BadRequest("This account is not related to any cart");

                account.Cart.UpdateCartItems();
                var json = new JsonResult(new CartViewModel()
                {
                    ID = account.CartID,
                    CreatedDate = account.Cart.CreationDate,
                    Items = account.Cart.CartItems.Select(x => new CartItemViewModel
                    {
                        ItemID = x.ItemID,
                        Quantity = x.Quantity,
                        UnitDiscount = x.UnitDiscount,
                        IsFree = x.IsFree, 
                        OfferID = x.OfferID,
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
        public async Task<IActionResult> UpdateInformation([FromBody]UpdateUserInformationViewModel userInfo)
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
                    account.Mobile = userInfo.PhoneNumber;
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
