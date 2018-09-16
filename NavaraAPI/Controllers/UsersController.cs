using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.Models;
using NavaraAPI.Services;
using NavaraAPI.ViewModels;
using SmartLifeLtd;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Enums;
using NavaraAPI.IServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NavaraAPI.Controllers
{
    /// <summary>
    /// The controller to work with the users sign in/out delete etc...
    /// </summary>
    public class UsersController : BaseApiController<IUsersService>
    {
        public UserManager<ApplicationUser> mUserManager { get; set; }
        /// <summary>
        /// The manager to work with the sign in process
        /// </summary>
        private SignInManager<ApplicationUser> mSignInManager;
    
        private NavaraDbContext _Context { set; get; }
        private  IHostingEnvironment env { set; get; }

        #region Constructer
        /// <summary>
        /// Default constructer
        /// </summary>
        public UsersController(NavaraDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUsersService usersService, IHostingEnvironment env
            ) : base(usersService)
        {
            this.env = env;
            _Context = context;
            mSignInManager = signInManager;
            mUserManager = userManager;
        }
        #endregion

        #region Account Requests
        /// <summary>
        /// Signs a user out of the application
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SignOut()
        {
            try
            {
                await mService.SignOut();
                return Ok("User has signOut");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Could not sign out please try again");
            }
        }

        /// <summary>
        /// Registers a new user to the database 
        /// </summary>
        /// <param name="userData">The informaiton of the user to store</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterUserDataModel userData)
        {
            if (ModelState.IsValid)
            {
                //If no data was vided
                if (userData == null || string.IsNullOrWhiteSpace(userData.Username))
                    //Return 400 bad request
                    return BadRequest("Please vide full information!");

                //Try to add the user
                try
                {
                    object result = null;
                    switch(userData.StationType)
                    {
                        case StationType.API:
                        case StationType.Web:
                        case StationType.Desktop:
                        case StationType.Mobile:
                        default:
                            result = await mService.Register(userData);
                            break;
                    }

                    //If the user got added right
                    if (result is string)
                        return Json(new { Token = result });

                    //Retrun a 500 errro
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }
            return BadRequest("Invaild information please check the sent information and try again");
        }

        /// <summary>
        /// Signs a user in and returns a token
        /// </summary>
        /// <param name="userData">The data of the user <see cref="SignInUserDataApiModel"/></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SignIn([FromBody]SignInUserDataApiModel userData)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(userData?.UserID))
                    return BadRequest("No UserID");
                try
                {
                    var result = await mService.SignIn(userData);
                    if (result is string)
                        return Json(new { Token = result });
                    return BadRequest(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }
            return BadRequest("Invaild information please check the sent information and try again");
        }

        [AuthorizeToken]
        [HttpGet]
        public async Task<IActionResult> ChangeLanguage(LanguageChange model)
        {
            try
            {
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _Context.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = _Context.Set<Account>().FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                if (_Context.Languages.Any(x => x.ID == model.LanguageID))
                {
                    account.LanguageID = model.LanguageID;
                    var path = Path.Combine(this.env.WebRootPath, "Languages/English.json");
                    if (!System.IO.File.Exists(path)) return BadRequest();
                    using (StreamReader reader = new StreamReader(path))
                    {
                        _Context.SaveChanges();
                        return Json(reader.ReadToEnd());
                    }
                }
                return StatusCode(StatusCodes.Status500InternalServerError, "Could not sign out please try again");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Could not sign out please try again");
            }
        }

        /// <summary>
        /// Changes the user password in the database
        /// </summary>
        /// <param name="newPassword">The new password to change</param>
        /// <returns></returns>
        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChange model)
        {
            if (ModelState.IsValid)
            {
                var result = await mService.ChangePassword(HttpContext.User.Identity.Name, model.OldPassword, model.NewPassword);
                if ((result as IdentityResult).Succeeded) return Ok(result);
                return StatusCode(StatusCodes.Status406NotAcceptable, result);
            }
            return BadRequest("Invalide data!");
        }

        /// <summary>
        /// Changes the user password in the database when the user forgetted it
        /// </summary>
        /// <param name="newPassword">The new password to change</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ChangeForgettedPassword([FromBody] ForgettedPasswordChange model)
        {
            if (ModelState.IsValid)
            {
                var userID = model.UserID;
                ApplicationUser user = await _Context.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                if (user == null) return null;
                string token = await mUserManager.GeneratePasswordResetTokenAsync(user);
                var result = await mUserManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (result.Succeeded)
                    return StatusCode(StatusCodes.Status200OK, result);
                //Retrun a 500 errro
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
            return BadRequest("Invalide data!");
        }

        /// <summary>
        /// Changes the user password in the database when the user forgetted it
        /// </summary>
        /// <param name="newPassword">The new password to change</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ResetPasswordOrder([FromBody] ResetPasswordOrder model)
        {
            if (ModelState.IsValid)
            {
                var userID = model.UserID;
                ApplicationUser user = await _Context.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                if (user == null) return null;
                string token = user.GenerateJwtToken(IoCCore.AppViewModel);
                if (user.UserName.IsValidEmail())
                {
                    EmailService.SendResetEmail(user.UserName, user.UserName, token);
                }
                else if (user.UserName.IsValidPhone())
                {
                    SMSService.SendResetSMS(user.UserName, user.UserName, token);
                }
                else return BadRequest("Unvalid email or password");
                return Ok();
            }
            return BadRequest("Invalide data!");
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string Token, string userID)
        {
            if (userID == null) return BadRequest("User Id is not related to any Account");
            ApplicationUser user = await _Context.Users.SingleOrDefaultAsync(item => item.UserName == userID);
            if (user == null) return null;
            string token = await mUserManager.GeneratePasswordResetTokenAsync(user);
            var result = await mUserManager.ResetPasswordAsync(user, token, "P@ssw0rd");
            return Content("Reset Successfuly! your new Password is: P@ssw0rd");
        }

        /// <summary>
        /// Changes the user password in the database
        /// </summary>
        /// <param name="newPassword">The new password to change</param>
        /// <returns></returns>
        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> ChangeForgettedPassword([FromBody] PasswordChange model)
        {
            if (ModelState.IsValid)
            {
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _Context.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                if (user == null) return null;
                string token = await mUserManager.GeneratePasswordResetTokenAsync(user);
                var result = await mUserManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (result.Succeeded)
                    return StatusCode(StatusCodes.Status200OK, result);
                //Retrun a 500 errro
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
            return BadRequest("Invalide data!");
        }

        /// <summary>
        /// Changes the user password in the database
        /// </summary>
        /// <param name="newPassword">The new password to change</param>
        /// <returns></returns>
        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> ConfirmAccount([FromBody] ConfirmAccountModel model)
        {
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            var result = await mService.ConfirmAccount(userID, model.Token);
            if (result == false) return StatusCode(StatusCodes.Status406NotAcceptable);
            return NoContent();
        }
       
        #endregion

        #region External Login
        [HttpPost]
        public IActionResult ExternalLogin(string provider)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "home");
            var properties = mSignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {

            var info = await mSignInManager.GetExternalLoginInfoAsync();
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var signinResult = await mSignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signinResult.Succeeded)
            {
                var signedInuser = await mService.GetAccountByUserID(email);

                return Ok(new SignInResultUserDataApiModel
                {
                    Username = signedInuser.User.UserName,
                    FirstName = signedInuser.Name,
                    Email = signedInuser.User.Email,
                    Token = signedInuser.User.GenerateJwtToken(IoCCore.AppViewModel)
                });
            }
            Account acc = new Account
            {
                Name = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                //LastName = info.Principal.FindFirstValue(ClaimTypes.Surname),
                CreationDate = DateTime.UtcNow,
                CashBack = 0
            };

            var user = new ApplicationUser
            {
                UserName = new Guid().ToString(),
                Email = email,
                AccountID = acc.ID
            };
            var result = await mUserManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await mUserManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    return Ok(new SignInResultUserDataApiModel
                    {
                        Username = user.UserName,
                        FirstName = acc.Name,
                        Email = user.Email
                        //Token = acc.GenerateJwtToken(),
                    });
                }
            }
            string errors = "";
            foreach (var error in result.Errors)
            {
                errors = string.Join(' ', errors, error.Description);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, errors);
        }
        #endregion
    }
}
