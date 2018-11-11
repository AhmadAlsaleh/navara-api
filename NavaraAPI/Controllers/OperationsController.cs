using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.ViewModels;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Data.Tables.Shared;
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
    public class OperationsController : SmartLifeLtd.Controllers.BaseOperationsController
    {
        public OperationsController(NavaraDbContext context, IHostingEnvironment env) : base(context, env)
        {
        }

        public ActionResult Chat()
        {
            return View("~/views/Index.cshtml");
        }
        //

    }
}
