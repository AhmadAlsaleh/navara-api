using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.Classes;
using NavaraAPI.ViewModels;
using SmartLifeLtd.API;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Data.Tables.Shared;
using SmartLifeLtd.Enums;
using SmartLifeLtd.Management.Interfaces;
using SmartLifeLtd.Services;

namespace NavaraAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class OrdersController : BaseController<Order>
    {
        readonly IConverter _converter;
        public OrdersController(NavaraDbContext context, IConverter converter)
            : base(context)
        {
            _converter = converter;
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
                    LocationText = model.LocationText,
                    Mobile = model.Mobile,
                    Remark = model.Remark,
                    Name = model.Name,
                    Status = OrderStatus.InProgress.ToString(),
                    UseWallet = model.UseWallet,
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
                if (order.UseWallet == true)
                {
                    int amount = ((int)(account.Wallet / 100)) * 100;
                    amount = Math.Min(amount, (account.Wallet as int?) ?? 0);
                    account.Wallet -= amount;
                    order.WalletAmount = amount;
                }

                this._context.Set<Order>().Add(order);
                await this._context.SaveChangesAsync();
                Thread UpdateAllInfo = new Thread(async () =>
                {
                    using (var context = new NavaraDbContext())
                    {
                        var orginalOrder = await context.Orders.Include("OrderItems").Include("Account").Include("Account.User")
                        .Include("OrderItems.Item").Include("OrderItems.Offer").FirstOrDefaultAsync(x => x.ID == order.ID);
                        await orginalOrder.FixMissingOfferItems(context);
                        await orginalOrder.UpdateOrder(context);
                        string pdfPath = await orginalOrder.CreatePDF(_converter, context as NavaraDbContext);
                        #region Send Email to Support
                        await EmailService.SendEmailToSupport($"New Order",
                            $"Order # {orginalOrder.Code}\r\n" +
                            $"Account: {orginalOrder.Account.Name}\r\n" +
                            $"Amount: {orginalOrder.OrderItems.Sum(x => x.Total)}\r\n" +
                            $"Date: {orginalOrder.CreationDate}\r\n" +
                            $"Days To Deliver: {orginalOrder.DaysToDeliver}\r\n" +
                            $"Items:\r\n{string.Join("\t | \t", orginalOrder.OrderItems.Select(x => x.Item?.Name + " : " + x.Quantity))}", Path.Combine("wwwroot", pdfPath));
                        string Mobile = "+" + order.Account.User.CountryCode +  order.Account.User.PhoneNumber;
                        if (!string.IsNullOrWhiteSpace(Mobile))
                        {
                            string message = $"Thank you for choosing Navara Store\r\nYour Order had been recieved\r\nOrder Code: {order.Code}\r\nOrder invoice: {Path.Combine("http://api.navarastore.com/", pdfPath.Replace("\\", "/"))}";
                            SMSService.SendWhatsApp(message, Mobile);
                        }
                        #endregion
                    }
                });
                UpdateAllInfo.Start();
                return Json(new { OrderCode = order.Code, DaysToDeliver = order.DaysToDeliver });
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
                    WalletAmount = order.WalletAmount,
                    DaysToDeliver = order.DaysToDeliver,
                    InvoicePath = order.InvoicePath,

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
                #region add test Notification
                INotificationContext AppContext = _context as INotificationContext;
                if (AppContext != null)
                {
                    Notification notification = new Notification()
                    {
                        Body = "There are also many informal uses of this kind of letter, though they may not necessarily be officially titled as a “notification”.",
                        ObjectID = (_context as NavaraDbContext)?.Items.FirstOrDefault().ID,
                        Subject = "New Item arived",
                        RelatedToEnum = NavaraNotificationRelatedTo.Item,
                        NotificationTypeEnum = NavaraNotificationType.NewItem
                    };
                    notification.AddStatus(AppContext, NotifyStatus.Sent, user.Id);
                    AppContext.Notifications.Add(notification);
                    AppContext.SaveChanges();
                }
                #endregion

                return json;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [AuthorizeToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                //if (!user.IsVerified) return StatusCode(StatusCodes.Status426UpgradeRequired);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                if (user == null) return BadRequest("Token is not related to any Account");

                var order = await _context.Set<Order>().Include("Account").Include("OrderItems").SingleOrDefaultAsync(x => x.ID == id);
                if (order == null) return NotFound("id is not realted to any order");
                if (order.AccountID != user.AccountID) return BadRequest("This Order is not related to the authorized user");
                order.Status = OrderStatus.Canceled.ToString();
                order.UpdatedDate = DateTime.Now;
                order.Account.Wallet += order.WalletAmount;
                _context.SubmitAsync();
                #region Send Email to Support
                EmailService.SendEmailToSupport($"Order # {order.Code} has been canceled",
                    $"Order # {order.Code} has been canceled\r\n" +
                    $"Account: {order.Account.Name}" +
                    $"Amount: {order.OrderItems.Sum(x => x.Total)}" +
                    $"Date: {order.CreationDate}" +
                    $"Days To Deliver: {order.DaysToDeliver}");
                #endregion
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
