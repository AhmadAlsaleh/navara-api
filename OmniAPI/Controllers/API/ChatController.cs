
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartLifeLtd.Data;
using OmniAPI.Controllers;


using SmartLifeLtd.Data.Tables.Omni;using SmartLifeLtd.Data.AspUsers;using SmartLifeLtd.Data.DataContexts;

using SmartLifeLtd.Data.Tables.Shared;


namespace OmniAPI.Controllers
{
    [Route("api/[controller]")]
    public class ChatController : BaseController<Chat>
    {
        public ChatController(OmniDbContext context, LogDbContext logContext, UserManager<ApplicationUser> userManager)
            : base(context, logContext, userManager)
        {

        }
    }
}
