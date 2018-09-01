using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NavaraAPI.Models;
using SmartLifeLtd;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Enums;
using SmartLifeLtd.IServices;
using System;
using System.Collections.Generic;
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
        #region Constructer
        /// <summary>
        /// Default constructer
        /// </summary>
        public UsersController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUsersService usersService
            ) : base(usersService)
        {
            mSignInManager = signInManager;
            mUserManager = userManager;
        }
        #endregion

        #region GET Requests
        /// <summary>
        /// Gets all the user in the database
        /// Will be removed when moving to realase 
        /// TODO:REMOVE
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                //try to add the users
                return Ok(await mService.GetUsers());
            }
            catch (Exception ex)
            {
                //if error happend return the exception
                StatusCode(StatusCodes.Status500InternalServerError, ex);
                return null;
            }
        }
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
        /// Gets the user informations that is saved in the database
        /// </summary>
        /// <returns></returns>
        [AuthorizeToken]
        [HttpGet]
        public async Task<IActionResult> GetUserInformation()
        {
            try
            {
                return Ok(await mService.GetUserInformation(HttpContext.User.Identity.Name));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }

        }
        #endregion

        #region POST Requests
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
                            result = await mService.RegisterUser(userData);
                            break;
                    }
                    //If the user got added right
                    if (result is RegisterResultApiModel)
                        return StatusCode(StatusCodes.Status201Created, result);

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
                //The error message that we want to return
                var errorMessage = "Invaild username or password";

                //Check if the data vided by user is not null
                if (userData?.UserID == null || string.IsNullOrWhiteSpace(userData.UserID))
                    return NotFound(errorMessage);
                try
                {
                    //if (await mUserManager.IsEmailConfirmedAsync(await mUserManager.FindByEmailAsync(userData.UsernameOrEmail)))
                        //Try to sign in the user
                        var userWithToken = await mService.SignInUser(userData);
                        //If we recived null then
                        if (userWithToken == null)
                            //Return not found
                            return NotFound(errorMessage);

                        //else return the user with his token
                        return Ok(userWithToken);
                    //return StatusCode(StatusCodes.Status406NotAcceptable, "Please confirm email!");
                }
                catch (NullReferenceException)
                {
                    return StatusCode(StatusCodes.Status404NotFound, errorMessage);
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, ex);
                }

            }
            return BadRequest("Invaild information please check the sent information and try again");
        }
        /// <summary>
        /// Updates all the user information in the database
        /// </summary>
        /// <param name="userInfo">The object that holds the new information</param>
        /// <returns></returns>
        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> UpdateUserInformation([FromBody]UpdateUserInformationViewModel userInfo)
        {
            //Check if the sent model is right 
            if (ModelState.IsValid)
            {
                try
                {
                    await mService.UpdateUserInformation(HttpContext.User.Identity.Name, userInfo);
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("Invaild information please check the sent information and try again");

        }
        /// <summary>
        /// Updates the user image
        /// </summary>
        /// <param name="userInfo">The object that holds the new information</param>
        /// <returns></returns>
        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> UpdateUserProfileImage([FromBody]UserfileImageUpdateApiModel data)
        {
            //Check if the sent model is right 
            if (ModelState.IsValid)
            {
                try
                {
                    //Tarek await mService.ChangeUserfilePictureBase64String(HttpContext.User.Identity.Name, data.Image);
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("The send data was not well formated!");

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
                var result = await mService.ChangeUserPassword(
                    new ChangePsswordViewModel
                    {
                        UserID = HttpContext.User.Identity.Name,
                        OldPassword = model.OldPassword,
                        NewPassword = model.NewPassword
                    });

                //If the user got added right
                if (result is Boolean)
                    return StatusCode(StatusCodes.Status200OK, result);

                //Retrun a 500 errro
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
            return BadRequest("Invalide data!");
        }
        #endregion

        #region Helper class
        /// <summary>
        /// A class for the password change action parameter
        /// </summary>
        public class PasswordChange
        {
            #region perties
            /// <summary>
            /// The old password of the user
            /// </summary>
            public string OldPassword { get; set; }
            /// <summary>
            /// The new user password
            /// </summary>
            public string NewPassword { get; set; }
            #endregion
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
                var signedInuser = await mService.GetAccountByEmailOrUsername(email);

                return Ok(new SignInResultUserDataApiModel
                {
                    Username = signedInuser.User.UserName,
                    FirstName = signedInuser.FirstName,
                    LastName = signedInuser.LastName,
                    Email = signedInuser.User.Email,
                    Token = signedInuser.User.GenerateJwtToken(IoCCore.AppViewModel),
                    ImagePath = signedInuser.ImagePath
                });
            }
            Account acc = new Account
            {
                FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                LastName = info.Principal.FindFirstValue(ClaimTypes.Surname),
                CreationDate = DateTime.UtcNow,
                ImagePath = "/images/profile-pic.png",
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
                        FirstName = acc.FirstName,
                        LastName = acc.LastName,
                        Email = user.Email,
                        //Token = acc.GenerateJwtToken(),
                        ImagePath = acc.ImagePath
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
