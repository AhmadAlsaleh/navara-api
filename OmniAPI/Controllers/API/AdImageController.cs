using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartLifeLtd.Data;
using OmniAPI.Controllers;
using SmartLifeLtd.Data.Tables.Omni;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IO;


namespace OmniAPI.Controllers
{
    [Route("api/[controller]")]
    public class AdImageController : BaseController<ADImage>
    {
        public AdImageController(OmniDbContext context, LogDbContext logContext, UserManager<ApplicationUser> userManager)
            : base(context, logContext, userManager)
        {

        }
    }
}
