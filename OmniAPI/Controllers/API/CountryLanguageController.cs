﻿
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartLifeLtd.Data;
using OmniAPI.Controllers;
using SmartLifeLtd.Data.Tables.Omni;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;

using SmartLifeLtd.Data.Tables;
using SmartLifeLtd.Data.Tables.Shared;

namespace OmniAPI.Controllers
{
    [Route("api/[controller]")]
    public class CountryLanguageController : BaseController<CountryLanguage>
    {
        public CountryLanguageController(OmniDbContext context, LogDbContext logContext, UserManager<ApplicationUser> userManager)
            : base(context, logContext, userManager)
        {

        }
    }
}
