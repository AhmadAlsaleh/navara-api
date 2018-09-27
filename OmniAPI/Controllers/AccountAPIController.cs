using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using OmniAPI.Controllers;
using OmniAPI.Models;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Omni;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Omni.Controllers.API
{

    [Route("api/[controller]/[Action]")]
    public class AccountAPIController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly OmniDbContext _context;


        public AccountAPIController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory
            , OmniDbContext context
           )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _context = context;
        }


        [HttpPost]
        [AllowAnonymous]
        //   [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody]LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByNameAsync(model.Email);
                    if (user == null) return Unauthorized();
                    Account AcountUser = _context?.Accounts?.Include("Area")?.Include("Area.City")?.Include("Area.City.Country")?.Include("AccountTokens").SingleOrDefault(x => x.ID == user.AccountID);

                    if (AcountUser == null) return Unauthorized();
                    var token = AcountUser.AccountTokens.FirstOrDefault();
                    if (AcountUser.Password == model.Password)
                    {
                        _logger.LogInformation(1, "User logged in.");

                        return Json(new
                        {
                            user.Email,
                            AcountUser.Password,
                            AcountUser.ID,
                            CountryID = "",
                            AcountUser.CreationDate,
                            token?.Token,
                            AcountUser.Phone,
                            AcountUser?.Name,
                            AcountUser.ImagePath
                        });
                    }
                    else
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Ok(new { Message = "Email or Password not Valid" });
                }
                catch (Exception e)
                {

                    return BadRequest(e.Message);
                }
                //     var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                
            }
            return Unauthorized();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> test()
        {
            return Ok("0");
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LoginWithFacebook([FromBody]FacebookLoginViewModel model)
        {
            Guid CountryID;

            Guid.TryParse(model.CountryID, out CountryID);
            var FacebookAccount= _context?.Accounts?.SingleOrDefault(s => s.FacebookID == model.FacebookID);
            if (FacebookAccount == null)
            {
                var user = new ApplicationUser { UserName = (!string.IsNullOrEmpty(model.Email))?model.Email:model.FacebookID, Email = model.FacebookID };
                var result = await _userManager.CreateAsync(user, "Smart"+model.FacebookID+"Shaker");
                if (result.Succeeded)
                {
                    Account EmarkUser = new Account() {FacebookID=model.FacebookID, Name = model.Name, CreationDate = DateTime.Now, Phone = model.Phone, JoinedDate = DateTime.Now, Password = "Smar"+model.FacebookID+"Shaker",ImagePath=model.ImagePath };
                    _context.Accounts.Add(EmarkUser);
                    await _context.SubmitAsync();
                    _context.Users.SingleOrDefault(x => x.Id == user.Id).AccountID = EmarkUser.ID;
                    await _context.SaveChangesAsync();
                    //         await _signInManager.SignInAsync(user, isPersistent: true);
                    _logger.LogInformation(3, "User created a new account with password.");


                    var code = Guid.NewGuid();
                    var token = new AccountToken()
                    {
                        Token = code.ToString(),
                        AccountID = EmarkUser.ID,
                        ClientType = "Computer",
                        TakenDate = DateTime.Now
                    };
                    _context.AccountTokens.Add(token);
                    await _context.SubmitAsync();
                    return Json(new
                    {
                        user.Email,
                        EmarkUser.Password,
                        EmarkUser.ID,
                        CountryID = "",
                        EmarkUser.CreationDate,
                        token?.Token,
                        EmarkUser.Phone,
                        EmarkUser?.Name,
                        EmarkUser.ImagePath
                    });
                }
         
   
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
            else
            {
                var token = _context?.AccountTokens?.SingleOrDefault(S => S.AccountID == FacebookAccount.ID).Token;
                return Json(new
                {
                    FacebookAccount.FacebookID,
                    FacebookAccount.Password,
                    FacebookAccount.ID,
                    CountryID = "",
                    FacebookAccount.CreationDate,
                    token,
                    FacebookAccount.Phone,
                    FacebookAccount?.Name,
                    FacebookAccount.ImagePath
                });
            }


            return BadRequest(ModelState);
    }
        [HttpPost]
        [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromBody]RegisterAPIViewModel model)
        {
            Guid CountryID;
           
            Guid.TryParse(model.CountryID, out CountryID);
            if (ModelState.IsValid)
            {

                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    Account EmarkUser = new Account() { Name = model.Name, CreationDate = DateTime.Now, Phone = model.Phone, JoinedDate = DateTime.Now,
                        Password = model.Password };
                    _context.Accounts.Add(EmarkUser);
                    await _context.SubmitAsync();
                    _context.Users.SingleOrDefault(x => x.Id == user.Id).AccountID = EmarkUser.ID;
                    await _context.SaveChangesAsync();
           //         await _signInManager.SignInAsync(user, isPersistent: true);
                    _logger.LogInformation(3, "User created a new account with password.");


                    var code = Guid.NewGuid();
                    var token = new AccountToken()
                    {
                        Token = code.ToString(),
                        AccountID = EmarkUser.ID,
                        ClientType = "Computer",
                        TakenDate = DateTime.Now
                    };
                    _context.AccountTokens.Add(token);
                    await _context.SubmitAsync();
                    return Ok(new { Token = code });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else ModelState.AddModelError("reg", "errorm");

            return BadRequest(ModelState);
        }



        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetAccountInfo(Guid ID)
        {
            try
            {

                var account = _context.Accounts.SingleOrDefault(x => x.ID == ID);
                var image = account.ImagePath;
                if (string.IsNullOrEmpty(account.ImagePath))
                {
                    image = $"/images/defaultPerson.gif";
                }
                return Ok(new
                {
                    image = image,
                    Phone = account.Phone ?? "",
                    Name = account.Name
                });

            }
            catch (Exception ex)

            {

                return BadRequest();
            }
           }

        [HttpPost]
        public async Task<IActionResult> UpdateInfo([FromBody]UserUpdateViewModel model)
        {
            try
            {
               
                var user =  await _userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                  //  ApplicationUser user = await _userManager.GetUserAsync(User);
                    var account = _context.Accounts.SingleOrDefault(x => x.ID == user.AccountID);
                    if (account == null) return BadRequest("No related account");
                    account.Name = model.Name;
                    account.Phone = model.Phone;
                    await _userManager.ChangePasswordAsync(user, model.Password, model.Password);
                    account.Password = model.Password;
                    await _context.SubmitAsync();
                    return Ok();
                }
                else return BadRequest("Not authorized");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public string SaveImage(string ImgStr, Account newAd)
        {
            String path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images\\AccountImages\\");

            //Check if directory exist
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }

            string imageName = newAd.ID+ ".jpg";

            //set the image path
            string imgPath = Path.Combine(path, imageName);

            byte[] imageBytes = Convert.FromBase64String(ImgStr);

            System.IO.File.WriteAllBytes(imgPath, imageBytes);
            return imageName;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UpdatePassword([FromBody]UpdatePasswordViewModel model)
        {
            try
            {
                if (model.AccountID == null || Guid.Empty == model.AccountID) return BadRequest("Account ID Is invalid");
                var account = _context.Accounts.SingleOrDefault(x => x.ID == model.AccountID);
                if (account == null) return BadRequest("No related account");
                if (model.OldPassword == account.Password)
                {
                    account.Password = model.NewPassword;
                    _context.SubmitAsync();
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateAccountInfo(  [FromBody]UserUpdateViewModel model)
        {
            try
            {
                if (model.AccountID == null || Guid.Empty == model.AccountID) return BadRequest("Account ID Is invalid");
                var account = _context.Accounts.SingleOrDefault(x => x.ID == model.AccountID);
                if (account == null) return BadRequest("No related account");
                account.Name = model.Name;
                account.Phone = model.Phone;
                if (model.ImagePath.ToLower() == "delete".ToLower())
                {
                    String path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", account.ImagePath);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    account.ImagePath = null;
                }
             else if (model.ImagePath?.Length > 0)
                {
                    if (!string.IsNullOrEmpty(account.ImagePath))
                    {
                        String path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", account.ImagePath);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                    
                    string filename = SaveImage(model.ImagePath, account);
                    if (!string.IsNullOrEmpty(filename))
                    {

                        account.ImagePath = "images\\AccountImages\\" + "\\" + filename;

                    }
                }
                await _context.SubmitAsync();
                return Ok(new {imagePath=account.ImagePath });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public async Task<IActionResult> ForgetPassword([FromBody]ForgotPasswordViewModel model)
        {
            ApplicationUser user = _userManager.FindByNameAsync(model.Email).Result;

            bool s = await _userManager.IsEmailConfirmedAsync(user);
            if (user == null)
            {

                return BadRequest("this email not valid");
            }

            var token = _userManager.GeneratePasswordResetTokenAsync(user).Result;

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

            //var message = new MimeMessage();
            //message.From.Add(new MailboxAddress("OLX Syria", "mohammad@smartlife-solutions.com"));
            //message.To.Add(new MailboxAddress("Test", model.Email));
            //message.Subject = "reset your password";
            //message.Body = new TextPart("plain") { Text = $"Please reset your password by clicking here: '{callbackUrl}'" };
            //using (var clint = new SmtpClient())
            //{
            //    clint.Connect("smtp.gmail.com", 587, false);
            //    clint.Authenticate("mohammad@smartlife-solutions.com", "Mohammed_95m");
            //    clint.Send(message);
            //    clint.Disconnect(true);
            //}
  
            return Json(new { Link = callbackUrl });
        }
        [HttpPost]
        public IActionResult ContactUS(Contact model)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var email = model.Email;
                    var subject = model.Subject;
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Contact US OLX", "mohammad@smartlife-solutions.com"));
                    message.To.Add(new MailboxAddress("Test", "mohammad@smartlife-solutions.com"));
                    message.Subject = model.Subject;
                    message.Body = new TextPart("plain") { Text = "Form Email :" + model.Email + "\n\n" + " Name : " + model.Name + "\n\n" + "Message Body : \n" + model.Message };
                    using (var clint = new SmtpClient())
                    {
                        clint.Connect("smtp.gmail.com", 587, false);
                        clint.Authenticate("mohammad@smartlife-solutions.com", "P@ssw0rd");
                        clint.Send(message);
                        clint.Disconnect(true);
                    }
                    return Ok();
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            return BadRequest();
        }
    }
}
