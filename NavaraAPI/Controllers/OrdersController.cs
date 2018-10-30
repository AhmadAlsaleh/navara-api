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
    public class OrdersController : BaseController<Order>
    {
        public OrdersController(NavaraDbContext context)
            : base(context)
        {
        }

        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody]OrderModel model)
        {
            try
            {
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                if (user == null) return BadRequest("Token is not related to any Account");
                Account account = _context.Set<Account>().Include(x => x.Cart)
                    .ThenInclude(x => x.CartItems).FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);

                Order order = new Order()
                {
                    AccountID = account.ID,
                    CreationDate = DateTime.Now,
                    FromTime = DateTime.Parse(model.FromTime),
                    ToTime = DateTime.Parse(model.ToTime),
                    Location = model.Location,
                    LocationRemark = model.LocationRemark,
                    Mobile = model.Mobile,
                    Remark = model.Remark,
                    Name = model.Name,
                    Status = OrderStatus.InProgress.ToString(),
                    UseWallet = model.UseWallet
                };
                foreach (var record in model.OrderItems)
                {
                    if (record == null) continue;
                    OrderItem orderItem = new OrderItem()
                    {
                        ItemID = record.OrderItemID,
                        Quantity = record.Quantity,
                        OfferID = record.OfferID
                    };
                    order.OrderItems.Add(orderItem);
                    if (account.Cart != null)
                    {
                        var item = account.Cart.CartItems.FirstOrDefault(x => x.ItemID == record.OrderItemID);
                        if (item != null) account.Cart.CartItems.Remove(item);
                    }
                }
                if (order.GenerateCode(_context as NavaraDbContext) == false)
                    return BadRequest("Error while generating Order Code");
                this._context.Set<Order>().Add(order);
                await this._context.SaveChangesAsync();
                await order.FixMissingOfferItems(_context);
                await order.UpdateOrder(_context);
                if (order.UseWallet == true) { }
                return Json(new { OrderCode = order.Code });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [ActionName("Get")]
        [AuthorizeToken]
        public override async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                if (user == null) return BadRequest("Token is not related to any Account");

                var order = await _context.Set<Order>()
                    .Include(x => x.OrderItems)
                        .ThenInclude(x => x.Item)
                    .SingleOrDefaultAsync(x => x.ID == id);
                if (order == null) return NotFound("id is not realted to any order");
                if (order.AccountID != user.AccountID) return BadRequest("This Order is not related to the authorized user");
                if (order.Number == null)
                {
                    order.GenerateCode(_context as NavaraDbContext);
                    await _context.SaveChangesAsync();
                }
                var json = new JsonResult(new OrderModel()
                {
                    ID = order.ID,
                    Name = order.Name,
                    FromTime = order.FromTime?.ToShortTimeString(),
                    Location = order.Location,
                    LocationRemark = order.LocationRemark,
                    Mobile = order.Mobile,
                    Remark = order.Remark,
                    ToTime = order.ToTime?.ToShortTimeString(),
                    Code = order.Code,
                    Date = order.CreationDate,
                    TotalPrices = order.OrderItems.Sum(y => (y.UnitPrice ?? 0) * (y.Quantity ?? 1)),
                    TotalDiscount = order.OrderItems.Sum(y => y.UnitDiscount ?? 0),
                    NetTotalPrices = order.OrderItems.Sum(y => y.Total ?? 0),
                    Status = order.Status,
                    UseWallet = order.UseWallet,

                    OrderItems = order.OrderItems.Select(y => new OrderItemModel
                    {
                        OrderItemID = y.ID,
                        Quantity = y.Quantity,
                        Name = y.Item?.Name,
                        UnitNetPrice = y.UnitNetPrice,
                        ThumbnailImagePath = y.Item?.ThumbnailImagePath,
                        Total = y.Total
                    }).ToList()
                });
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
