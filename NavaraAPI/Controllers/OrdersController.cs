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
                Account account = _context.Set<Account>().FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);

                Order order = new Order()
                {
                    AccountID = account.ID,
                    FromTime = DateTime.Parse(model.FromTime),
                    ToTime = DateTime.Parse(model.ToTime),
                    Location = model.Location,
                    LocationRemark = model.LocationRemark,
                    Mobile = model.Mobile,
                    Remark = model.Remark,
                    Name = model.Name,
                    Status = OrderStatus.InProgress.ToString()
                };
                foreach (var record in model.OrderItems)
                {
                    OrderItem orderItem = new OrderItem()
                    {
                        ItemID = record.OrderItemID,
                        Quantity = record.Quantity
                    };
                    order.OrderItems.Add(orderItem);
                }
                order.GenerateCode(_context as NavaraDbContext);
                this._context.Set<Order>().Add(order);
                await this._context.SaveChangesAsync();
                return Json(new { OrderCode = order.Code });
            }
            catch (Exception ex)
            {
                return BadRequest();
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

                var order = await _context.Set<Order>().Include(x => x.OrderItems).SingleOrDefaultAsync(x => x.ID == id);
                if (order == null) return NotFound("id is not realted to any order");
                if (order.AccountID != user.AccountID) return BadRequest("This Order is not related to the authorized user");
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
                    OrderItems = order.OrderItems.Select(y => new OrderItemModel
                    {
                        OrderItemID = y.ID,
                        Quantity = y.Quantity
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
