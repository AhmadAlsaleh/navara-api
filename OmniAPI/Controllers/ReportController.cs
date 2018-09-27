using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OmniAPI.Models;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Omni;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OmniWeb.Controllers.Mobile
{
    [Route("api/[controller]/[Action]")]
    public class ReportController :Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OmniDbContext _context;
        public ReportController(UserManager<ApplicationUser> userManager, OmniDbContext context)
        {
            _context = context;
            _userManager = userManager;

        }
        [HttpPost]
        public async Task<IActionResult> NewReport([FromBody] ReportViewModel model)
        {
            try
            {
                if (model == null) return BadRequest();
                var Account = _context.Accounts.SingleOrDefault(S => S.ID == model.UserID);
                if (Account == null) return BadRequest("Account not Found");
                var AD = _context.ADs?.SingleOrDefault(S => S.ID == model.AddID);
                if (AD == null) return BadRequest("Ad Not Found");
                Report NewReport = new Report
                {
                    AD = AD,
                    ReportSender = Account,
                    Body = model.Body,
                    CreationDate=DateTime.UtcNow
                };
                _context.Reports.Add(NewReport);
              await  _context.SubmitAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


    }
}
