using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using OmniAPI.Models;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Omni;
using SmartLifeLtd.Sync;
using SmartLifeLtd.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class OperationsController : Controller
    {    
        private OmniDbContext _Context { set; get; }
        private IHostingEnvironment env { set; get; }

        #region Constructer
        /// <summary>
        /// Default constructer
        /// </summary>
        public OperationsController(OmniDbContext context, IHostingEnvironment env)
        {
            _Context = context;
            this.env = env;
        }
        #endregion

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
        [AuthorizeToken]
        public async Task<IActionResult> Report([FromBody] ReportDataModel model)
        {
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _Context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _Context.Set<Account>()
                .Include(x => x.ADs)
                    .ThenInclude(x => x.ADImages)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            if (model == null) return BadRequest("No data provided");
            var AD = _Context.ADs.SingleOrDefault(S => S.ID == model.AddID);
            if (AD == null) return BadRequest("Ad ID is not related to any ad");

            Report NewReport = new Report
            {
                ADID = AD.ID,
                ReportSenderID = account.ID,
                Body = model.Body,
                CreationDate = DateTime.UtcNow
            };
            _Context.Reports.Add(NewReport);
            await _Context.SubmitAsync();
            return this.Content("Report has been sent successfully");
        }
    }
}
