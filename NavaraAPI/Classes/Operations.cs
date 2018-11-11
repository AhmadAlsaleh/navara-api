using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLifeLtd.API;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables;
using SmartLifeLtd.Data.Tables.Navara;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavaraAPI.Classes
{
    public static class Operations
    {
        [AuthorizeToken]
        [HttpPost]
        public static async Task<ApplicationUser> GetUser<T>(this BaseController<T> controller) where T : DatabaseObject
        {
            try
            {
                var userID = controller.HttpContext.User.Identity.Name;
                if (userID == null) return null;
                ApplicationUser user = await controller._context.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                return user;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<string> CreatePDF(this Order order, IConverter converter, NavaraDbContext context)
        {
            try
            {
                string Root = "wwwroot";
                string pdfFoler = $@"Orders";
                if (!Directory.Exists(Path.Combine(Root, pdfFoler))) Directory.CreateDirectory(Path.Combine(Root, pdfFoler));
                string pdfFile = Path.Combine(Root, pdfFoler, $"{order.Code}.pdf");
                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10 },
                    DocumentTitle = "Order " + order.Code,
                    Out = pdfFile
                };
                var orginal = context.Orders.Include("OrderItems").Include("OrderItems.Item")
                     .Include("OrderItems.Item.ItemCategory").Include("Account")
                     .Include("Account.User")
                     .FirstOrDefault(x => x.ID == order.ID);
                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = OrderTemplate(orginal),
                    //WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Styles", "OrderStyle.css") },
                    HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Left = "Navara Store, Electronic store #1 in Syria" },
                    FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Invoice was created on a computer and is valid without the signature and seal." +
                    "\r\nFor more details visit our website: www.navarastore.com" }
                };

                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };

                converter.Convert(pdf);
                order.InvoicePath = Path.Combine(pdfFoler, $"{order.Code}.pdf");
                await context.SubmitAsync();
                return order.InvoicePath;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static string OrderTemplate(Order order)
        {
            string template = "";
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", "Order.html");
            if (File.Exists(path))
                template = File.ReadAllText(path);

            var items = new StringBuilder();
            string itemTemplate = @"<tr>
                    <td class='service'>ITEMCATEGORY</td>
                    <td class='desc'>ITEMNAME</td>
                    <td class='unit'>ITEMPRICE</td>
                    <td class='qty'>ITEMQUANTITY</td>
                    <td class='total'>ITEMTOTAL</td>
                </tr>
";
            foreach (var orderItem in order.OrderItems)
            {
                string item = itemTemplate;
                item = item.Replace("ITEMCATEGORY", orderItem.Item?.ItemCategory?.Name);
                item = item.Replace("ITEMNAME", orderItem.Item?.Name);
                item = item.Replace("ITEMPRICE", (orderItem.Item?.Price ?? 0).ToString("n0") + " S.P");
                item = item.Replace("ITEMQUANTITY", orderItem?.Quantity?.ToString());
                item = item.Replace("ITEMTOTAL", (orderItem?.Total ?? 0).ToString("n0") + " S.P");
                items.Append(item);
            }
            template = template.Replace("ORDERCODE", order.Code);
            template = template.Replace("ORDERSUBTOTAL", order.OrderItems.Sum(x => (x.UnitPrice ?? 0) * (x.Quantity ?? 1)).ToString("n0") + " S.P");
            template = template.Replace("DISCOUNTAMOUNT", order.OrderItems.Sum(x => (x.UnitDiscount ?? 0) * (x.Quantity ?? 1)).ToString("n0") + " S.P");
            template = template.Replace("ORDERGRANDTOTAL", order.OrderItems.Sum(x => x.Total ?? 0).ToString("n0") + " S.P");
            template = template.Replace("COMPANYNAME", "NAVARA STORE");
            template = template.Replace("COMPANYADDRESS", "8th Street, Latakia, Syria");
            template = template.Replace("COMPANYMOBILE", "+963 943 877 890");
            template = template.Replace("COMPANYEMAIL", "support@navarastore.com");
            template = template.Replace("CLIENTNAME", order.Account?.Name);
            template = template.Replace("CLIENTADDRESS", order.LocationText + $" ({order.LocationRemark})");
            template = template.Replace("CLIENTEMAIL", order.Account?.User?.Email);
            template = template.Replace("CLIENTMOBILE", order.Account?.User?.PhoneNumber);
            template = template.Replace("ORDERDATE", order.CreationDate.ToString("dd/MM/yyyy"));
            template = template.Replace("TITLE", order.Code);
            template = template.Replace("ORDERITEMS", items.ToString());
            template = template.Replace("NOTICETEXT", "You can return any item within 7 days for an order delivery date (Unless the item is used).");
            template = template.Replace("LOGOURL", "http://api.navarastore.com/logo.jpg");

            return template;
        }
    }
}
