using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartLifeLtd.Data;
using OmniAPI.Controllers;
using SmartLifeLtd.Data.Tables.Omni;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables;
using SmartLifeLtd.Data.Tables.Log;

namespace OmniAPI.Controllers
{
    [Route("api/[controller]")]
    public class LogErrorController : BaseController<LogError>
    {
        public LogErrorController(OmniDbContext context, LogDbContext logContext, UserManager<ApplicationUser> userManager)
            : base(context, logContext, userManager)
        {

        }
    }
}
