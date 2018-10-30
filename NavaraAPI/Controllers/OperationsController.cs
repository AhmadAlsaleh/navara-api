using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.ViewModels;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Enums;
using SmartLifeLtd.Management.Interfaces;
using SmartLifeLtd.Sync;
using SmartLifeLtd.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class OperationsController : SmartLifeLtd.Controllers.OperationsController
    {
        public OperationsController(NavaraDbContext context, IHostingEnvironment env) : base(context, env)
        {
        }

        public ActionResult Chat()
        {
            return View("~/views/Index.cshtml");
        }

        [HttpPost]
        public IActionResult Search([FromBody]SearchViewModel model)
        {
            try
            {
                var AppContext = this._Context as ISearchContext;
                if (AppContext == null) return BadRequest("App not Support Save Search");
                AppContext.SearchHistories.Add(new SearchHistory()
                {
                    SearchDate = DateTime.Now,
                    SearchText = model.SearchText
                });
                AppContext.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
    /*[Route("[controller]/[action]")]
    public class OperationsController : Controller
    {    
        private NavaraDbContext _Context { set; get; }
        private IHostingEnvironment env { set; get; }

        #region Constructer
        /// <summary>
        /// Default constructer
        /// </summary>
        public OperationsController(NavaraDbContext context, IHostingEnvironment env)
        {
            _Context = context;
            this.env = env;
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> DownloadFile([FromBody]FileViewModel item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.FilePath)) return BadRequest();
            var path = Path.Combine(this.env.WebRootPath, item.FilePath);
            if (!System.IO.File.Exists(path)) return NotFound("File is not exist");
            try
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, MMITypes.GetContentType(path), Path.GetFileName(path));
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ContactUs([FromBody]ContactUsViewModel model)
        {
            try
            {
                //EmailService.SendEmail(model.Name, model.Email, model.Message, "Contact us");
                return this.Content("Message has been sent successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Search([FromBody]SearchViewModel model)
        {
            try
            {
                this._Context.SearchHistories.Add(new SearchHistory()
                {
                    SearchDate = DateTime.Now,
                    SearchText = model.SearchText
                });
                this._Context.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AuthorizeToken]
        public async Task<IActionResult> GetNotifications()
        {
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _Context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            if (user == null) return BadRequest("Token is not related to any Account");
            Account account = _Context.Set<Account>().Include(x => x.Cart)
                .ThenInclude(x => x.CartItems).FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;

            try
            {
                List<dynamic> data = new List<dynamic>();
                var notifications = this._Context.Notifications.Include("NotificationStatuses")
                    .Where(x => x.NotificationStatuses.Any(y => y.UserID == account.ID));
                foreach(var notify in notifications)
                {
                    var Statuses = notify.NotificationStatuses;
                    if(Statuses.Any(x => x.Status == NotifyStatus.Sent.ToString()) &&
                    !Statuses.Any(x => x.Status == NotifyStatus.Recieved.ToString()))
                    {
                        data.Add(new
                        {
                            Subject = notify.Subject,
                            Body = notify.Body,
                            RelatedTo = notify.RelatedTo,
                            Type = notify.NotificationType,
                            ID = notify.ID,
                            RelatedID = notify.ObjectID
                        });
                        notify.AddStatus(_Context, NotifyStatus.Recieved, account.ID);
                    }
                }
                this._Context.SaveChanges();
                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AuthorizeToken]
        [HttpGet("{ID}")]
        public async Task<IActionResult> SeeNotification(Guid ID)
        {
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _Context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            if (user == null) return BadRequest("Token is not related to any Account");
            Account account = _Context.Set<Account>().Include(x => x.Cart)
                .ThenInclude(x => x.CartItems).FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;

            try
            {
                var notification = await this._Context.Notifications.Include("NotificationStatuses").FirstOrDefaultAsync(x => x.ID == ID);
                if (notification == null) return BadRequest("ID is not related to any Notification");
                notification.AddStatus(_Context, NotifyStatus.Seen);
                this._Context.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        public IActionResult Exception([FromBody]ExceptionViewModel model)
        {
            try
            {
                this._Context.SearchHistories.Add(new SearchHistory()
                {
                    SearchDate = DateTime.Now,
                    SearchText = model.SearchText
                });
                this._Context.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }*/
}
