using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NavaraAPI.Controllers
{
    /// <summary>
    /// A generic base class which holds the service class of the specified type
    /// </summary>
    /// <typeparam name="IService">The service interface that holds the functions</typeparam>
    [Route("[controller]/[action]/{id?}")]
    public class BaseApiController<IService> : Controller
    {
        #region Protected members
        /// <summary>
        /// The service that will serve the controllers needs
        /// </summary>
        protected IService mService { get; private set; }
        #endregion

        #region Constructer
        /// <summary>
        /// Default constructer
        /// </summary>
        public BaseApiController(IService service)
        {
            mService = service;
        }
        #endregion
    }
}
