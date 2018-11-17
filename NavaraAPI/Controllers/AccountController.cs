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
using SmartLifeLtd.Controllers;
using SmartLifeLtd.IServices;
using SmartLifeLtd.Data.Tables.Shared;

namespace NavaraAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : BaseApiController<IUsersService>
    {
        private NavaraDbContext _Context { set; get; }
        public AccountController(NavaraDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUsersService userService) : base(userService)
        {
            _Context = context;
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
                                .ThenInclude(x => x.ItemImages)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (account == null) return BadRequest("Error in get user or account data");
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);

                if (account.Cart == null)
                {
                    account.Cart = new Cart()
                    {
                        CreationDate = DateTime.Now,
                        TotalAmount = 0,
                        LastPurchase = DateTime.Now,
                    };
                    await _Context.SaveChangesAsync();
                }
                //return BadRequest("This account is not related to any cart");

                await account.Cart.UpdateCart(_Context);
                var json = new JsonResult(new CartViewModel()
                {
                    CreatedDate = account.Cart.CreationDate,
                    AccountID = account.ID,
                    Items = account.Cart.CartItems?.Select(x => new CartItemViewModel
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
                        UnitPrice = x.UnitPrice,
                        CashBack = x.Item?.CashBack,
                        AccountID = x.Item?.AccountID

                    })?.ToList()
                });
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [AuthorizeToken]
        public async Task<IActionResult> GetOwnItems()
        {
            try
            {
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _Context.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                if (user == null) return BadRequest("Error in get user or account data");
                Account account = _Context.Set<Account>()
                    .Include(x => x.Items)
                        .ThenInclude(x => x.ItemImages)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (account == null) return BadRequest("Error in get user or account data");

                var data = account.Items.ToList();
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
                    IsEnable = x.IsEnable
                }));
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        [AuthorizeToken]
        public async Task<IActionResult> DeactivateItem(Guid id)
        {
            try
            {
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _Context.Users.SingleOrDefaultAsync(x => x.UserName == userID);
                if (user == null) return BadRequest("Error in get user or account data");
                Account account = _Context.Set<Account>()
                    .Include(x => x.Items)
                        .ThenInclude(x => x.ItemImages)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (account == null) return BadRequest("Error in get user or account data");

                var item = account.Items.FirstOrDefault(x => x.ID == id);
                if (item != null) item.IsEnable = !(item.IsEnable ?? false);
                _Context.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [AuthorizeToken]
        public async Task<IActionResult> GetWallet()
        {
            try
            {
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _Context.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = _Context.Set<Account>().FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                return Ok(account.Wallet);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet, AuthorizeToken] public async Task<IActionResult> GetInformation() { return RedirectToAction("GetInformation", "Users"); }
        [HttpGet, AuthorizeToken] public async Task<IActionResult> UpdateInformation([FromBody]UpdateUserInformationDataModel userInfo) { return RedirectToAction("UpdateInformation", "Users", userInfo); }
    }
}
