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
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = _context.Set<Account>()
                    .Include(x => x.Cart)
                        .ThenInclude(x => x.CartItems)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);

                var cartItem = account.Cart?.CartItems.FirstOrDefault(x => x.ItemID == model.ItemID);
                if (cartItem == null) return NoContent();

                if(cartItem.OfferID != null)
                {
                    var removeItem = _context.Set<CartItem>().Where(x => x.OfferID == cartItem.OfferID);
                    _context.Set<CartItem>().RemoveRange(removeItem);
                }
                _context.Set<CartItem>().Remove(cartItem);
                await _context.SaveChangesAsync();
                account.Cart.UpdateCartItems();
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
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = _context.Set<Account>()
                    .Include(x => x.Cart)
                        .ThenInclude(x => x.CartItems)
                            .ThenInclude(x => x.Item)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);

                if (account.Cart == null)
                {
                    account.Cart = new Cart()
                    {
                        CreationDate = DateTime.Now,
                        TotalAmount = 0,
                        LastPurchase = DateTime.Now,
                    };
                }
                #region Add Item
                var cartItem = account.Cart.CartItems.FirstOrDefault(x => x.ItemID == model.ItemID);
                if (cartItem == null)
                {
                    cartItem = (new CartItem()
                    {
                        ItemID = model.ItemID,
                        CreationDate = DateTime.Now
                    });
                    account.Cart.CartItems.Add(cartItem);
                }
                cartItem.Quantity = model.Quantity;
                #endregion
                
                await _context.SaveChangesAsync();
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
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = _context.Set<Account>()
                    .Include(x => x.Cart)
                        .ThenInclude(x => x.CartItems)
                            .ThenInclude(x => x.Item)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);

                if (account.Cart == null)
                {
                    account.Cart = new Cart()
                    {
                        CreationDate = DateTime.Now,
                        TotalAmount = 0,
                        LastPurchase = DateTime.Now,
                    };
                }

                #region Add Offer 
                var Offer = _context.Set<Offer>()
                    .Include(x => x.OfferItems)
                    .Include(x => x.Item)
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
                        var cartItem = account.Cart.CartItems.FirstOrDefault(x => x.ItemID == item.ID && x.OfferID == Offer.ID);
                        if (cartItem == null)
                        {
                            cartItem = (new CartItem()
                            {
                                ItemID = item.ID,
                                CreationDate = DateTime.Now
                            });
                            account.Cart.CartItems.Add(cartItem);
                        }
                        cartItem.Quantity = model.Quantity;
                        cartItem.UnitPrice = item.Price;
                        cartItem.UnitDiscount = Offer.Discount;
                        cartItem.UnitNetPrice = item.Price - (item.Price * Offer.Discount / 100.0);
                        cartItem.Total = cartItem.Quantity * cartItem.UnitNetPrice;
                        cartItem.IsFree = false;
                        cartItem.OfferID = Offer.ID;
                        #endregion
                        break;
                    case OfferType.Free:
                        #region  Free Offer
                        var cartItemMain = account.Cart.CartItems.FirstOrDefault(x => x.ItemID == Offer.ItemID && x.OfferID == Offer.ID);
                        if (cartItemMain == null)
                        {
                            cartItemMain = (new CartItem()
                            {
                                ItemID = Offer.ItemID,
                                CreationDate = DateTime.Now
                            });
                            account.Cart.CartItems.Add(cartItemMain);
                        }
                        cartItemMain.Quantity = model.Quantity;
                        cartItemMain.UnitNetPrice = cartItemMain.UnitPrice = Offer.Item.Price;
                        cartItemMain.UnitDiscount = 0;
                        cartItemMain.Total = cartItemMain.Quantity * cartItemMain.UnitNetPrice;
                        cartItemMain.IsFree = false;
                        cartItemMain.OfferID = Offer.ID;
                        foreach (var offerItem in Offer.OfferItems)
                        {
                            var itemFree = await _context.Set<Item>().FirstOrDefaultAsync(x => x.ID == offerItem.ID);
                            var cartItemFree = account.Cart.CartItems.FirstOrDefault(x => x.ItemID == itemFree.ID && x.OfferID == Offer.ID);
                            if (cartItemFree == null)
                            {
                                cartItemFree = (new CartItem()
                                {
                                    ItemID = itemFree.ID,
                                    CreationDate = DateTime.Now
                                });
                                account.Cart.CartItems.Add(cartItemFree);
                            }
                            cartItemFree.Quantity = model.Quantity;
                            cartItemFree.UnitPrice = itemFree.Price;
                            cartItemFree.UnitDiscount = Offer.Discount;
                            cartItemFree.UnitNetPrice = itemFree.Price - (itemFree.Price * Offer.Discount / 100.0);
                            cartItemFree.Total = cartItemFree.Quantity * cartItemFree.UnitNetPrice;
                            cartItemFree.IsFree = true;
                            cartItemFree.OfferID = Offer.ID;
                        }
                        #endregion
                        break;
                    case OfferType.Set:

                        break;
                    case OfferType.None:
                    default:
                        break;
                }
                #endregion

                await _context.SaveChangesAsync();
                account.Cart.UpdateCartItems();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
