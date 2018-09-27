using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.ViewModels;
using SmartLifeLtd.API;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Enums;

namespace NavaraAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class CartController : BaseController<Cart>
    {
        public CartController(NavaraDbContext context)
            : base(context)
        {

        }

        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart([FromBody]CartItemViewModel model)
        {
            try
            {
                #region Check user
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = _context.Set<Account>()
                    .Include(x => x.Cart)
                        .ThenInclude(x => x.CartItems)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);
                #endregion

                var cartItem = account.Cart?.CartItems.FirstOrDefault(x => x.ItemID == model.ItemID);
                if (cartItem == null) return NoContent();

                if (cartItem.OfferID != null)
                {
                    var removeItem = _context.Set<CartItem>().Where(x => x.OfferID == cartItem.OfferID);
                    _context.Set<CartItem>().RemoveRange(removeItem);
                }
                _context.Set<CartItem>().Remove(cartItem);
                await _context.SaveChangesAsync();
                await account.Cart.UpdateCart(_context);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> AddItemToCart([FromBody]CartItemViewModel model)
        {
            try
            {
                #region Check user
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(x => x.UserName == userID);
                Account account = _context.Set<Account>()
                    .Include(x => x.Cart).ThenInclude(x => x.CartItems)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);
                #endregion

                var it = _context.Set<Item>().FirstOrDefault(x => x.ID == model.ItemID);
                if (it == null) return BadRequest("No Item related to this ItemID");

                #region Create cart for the first time
                if (account.Cart == null)
                {
                    account.Cart = new Cart()
                    {
                        CreationDate = DateTime.Now,
                        TotalAmount = 0,
                        LastPurchase = DateTime.Now,
                    };
                }
                else
                {
                    foreach(var ci in account.Cart.CartItems)
                    {
                        if (ci.ItemID == null || ci.UnitPrice == null)
                            _context.Set<CartItem>().Remove(ci);
                    }
                }
                #endregion
                
                #region Add Item
                var cartItem = account.Cart.CartItems.FirstOrDefault(x => x.ItemID == model.ItemID);
                if (cartItem == null) // If item is not exist add it
                {
                    cartItem = (new CartItem()
                    {
                        ItemID = model.ItemID,
                        CreationDate = DateTime.Now
                    });
                    account.Cart.CartItems.Add(cartItem);
                }
                cartItem.Quantity = model.Quantity;
                cartItem.OfferID = null;
                #endregion

                await _context.SaveChangesAsync();
                await account.Cart.UpdateCart(_context);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> AddOfferToCart([FromBody]CartOfferViewModel model)
        {
            try
            {
                #region Check user
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = _context.Set<Account>()
                    .Include(x => x.Cart)
                        .ThenInclude(x => x.CartItems)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);
                #endregion

                var off = _context.Set<Offer>().FirstOrDefault(x => x.ID == model.OfferID);
                if (off == null) return BadRequest("No Offer related to this OfferID");

                #region Create cart for the first time
                if (account.Cart == null)
                {
                    account.Cart = new Cart()
                    {
                        CreationDate = DateTime.Now,
                        TotalAmount = 0,
                        LastPurchase = DateTime.Now,
                    };
                }
                else
                {
                    foreach (var ci in account.Cart.CartItems)
                    {
                        if (ci.ItemID == null || ci.UnitPrice == null)
                            _context.Set<CartItem>().Remove(ci);
                    }
                }
                #endregion

                #region Add Offer 
                var Offer = _context.Set<Offer>().Include(x => x.OfferItems)
                    .FirstOrDefault(x => x.ID == model.OfferID);
                OfferType offerType = OfferType.None;
                Enum.TryParse<OfferType>(Offer.OfferType, out offerType);
                var removeItem = _context.Set<CartItem>().Where(x => x.OfferID == model.OfferID);
                _context.Set<CartItem>().RemoveRange(removeItem);
                switch (offerType)
                {
                    case OfferType.Discount:
                        #region Discount Offer
                        var item = await _context.Set<Item>().FirstOrDefaultAsync(x => x.ID == Offer.ItemID);
                        var cartItem = new CartItem()
                        {
                            ItemID = item.ID,
                            CreationDate = DateTime.Now,
                            Quantity = model.Quantity,
                            OfferID = Offer.ID
                        };
                        account.Cart.CartItems.Add(cartItem);
                        #endregion
                        break;
                    case OfferType.Free:
                        #region  Free Offer
                        var cartItemMain = new CartItem()
                        {
                            ItemID = Offer.ItemID,
                            CreationDate = DateTime.Now,
                            Quantity = model.Quantity,
                            OfferID = Offer.ID
                        };
                        account.Cart.CartItems.Add(cartItemMain);
                        foreach (var offerItem in Offer.OfferItems)
                        {
                            var itemFree = await _context.Set<Item>().FirstOrDefaultAsync(x => x.ID == offerItem.ItemID);
                            if (itemFree == null) continue;
                            var cartItemFree = new CartItem()
                            {
                                ItemID = itemFree.ID,
                                CreationDate = DateTime.Now,
                                Quantity = model.Quantity,
                                OfferID = Offer.ID
                            };
                            account.Cart.CartItems.Add(cartItemFree);
                        }
                        #endregion
                        break;
                    case OfferType.Set:
                    case OfferType.None:
                    default:
                        break;
                }
                #endregion

                await _context.SaveChangesAsync();
                await account.Cart.UpdateCart(_context);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
