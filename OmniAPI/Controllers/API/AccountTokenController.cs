﻿
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Omni;

namespace OmniAPI.Controllers
{
    [Route("api/[controller]")]
    public class AccountTokenController : BaseController<AccountToken>
    {
        public AccountTokenController(OmniDbContext context, LogDbContext logContext, UserManager<ApplicationUser> userManager)
            : base(context, logContext, userManager)
        {

        }
    }
}
