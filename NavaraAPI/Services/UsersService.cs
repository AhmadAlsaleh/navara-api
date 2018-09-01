using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartLifeLtd.IServices;
using NavaraAPI.Models;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartLifeLtd.ViewModels;
using SmartLifeLtd.Services;
using SmartLifeLtd.IServices;
using SmartLifeLtd;

namespace NavaraAPI.Services
{
    /// <summary>
    /// Holds the business logic for interaction with an account
    /// </summary>
    public class UsersService : BaseService, IUsersService
    {
        #region Private members
        /// <summary>
        /// The instance to work with sign in/out for a user
        /// </summary>
        private readonly SignInManager<ApplicationUser> mSignInManager;
        /// <summary>
        /// The instance to work with create,delete,serach etc...
        /// </summary>
        private readonly UserManager<ApplicationUser> mUserManger;
        #endregion

        #region Constructer
        /// <summary>
        /// Default constructer
        /// </summary>
        /// <param name="context">The injected database</param>
        /// <param name="signInManager">Will be injected</param>
        /// <param name="userManager">Will be injected</param>
        public UsersService(NavaraDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) :
            base(context)
        {
            mSignInManager = signInManager;
            mUserManger = userManager;

        }
        #endregion

        public async Task<Account> GetAccountByID(Guid id)
        {
            return await mContext.Accounts.Include(item => item.User).SingleOrDefaultAsync(ac => ac.ID == id);
        }
        public async Task<Account> GetAccountByEmailOrUsername(string emailOrUsername)
        {
            //Check if is an email
            var isEmail = emailOrUsername.Contains('@');
            try
            {

                var account = (await mContext.Accounts.Include(user => user.User).SingleOrDefaultAsync(user => user.User != null && (user.User.Email == emailOrUsername || user.User.UserName == emailOrUsername)));
                return account;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Object> RegisterUser(RegisterUserDataModel userData)
        {
            var newUser = new ApplicationUser
            {
                UserName = userData.Username,
                Email = userData.Email,
                PhoneNumber = userData.PhoneNumber,
                TwoFactorEnabled = false,
                PhoneNumberConfirmed = false,
                EmailConfirmed = false,
                LockoutEnabled = false
            };
            //Creates and addes a user to the database
            var result = await mUserManger.CreateAsync(newUser, userData.Password);

            if (result.Succeeded)
            {
                Account acc = new Account
                {
                    FirstName = userData.FirstName ?? userData.Email.Split('@')[0],
                    LastName = userData.LastName,
                    DateOfBirth = userData.DataOfBirth,
                    Gender = userData.Gender,
                    CreationDate = DateTime.UtcNow,
                    ImagePath = "/images/profile-pic.png",
                };
                mContext.Accounts.Add(acc);
                await mContext.SaveChangesAsync();

                //Get the added user
                var registerdUser = await mUserManger.FindByNameAsync(userData.Username);
                //Assign the id to the user
                registerdUser.AccountID = acc.ID;
                acc.UserID = registerdUser.Id;

                //Get the account for this user
                //registerdUser.AccountID = registerdUser.AccountID;
                var token = await mUserManger.GenerateEmailConfirmationTokenAsync(registerdUser);
                #region TAREK
                /* //TODO :Add email vertification
                var devLocal = "http://localhost:51804";
                var RelaseDomain = "http://requestapi.smartlife-solutions.com";

                 EmailService.SendEmail(registerdUser.Account.FirstName, token, "Confirme email", registerdUser.Email, registerdUser.Id, domainName: RelaseDomain);

                //Try to add images if there is
                Tarek if (withImage)
                    try
                    {
                        //Set the image and thumbnail path
                        acc.ImagePath = await FileHelpers.SaveImage(userData.Image, IoCCore.AppViewModel.ASPApplicaitonRootDirectory, FilePathsForWWWRoot.AccountImages, registerdUser.AccountID);

                    }
                    catch (Exception)
                    {
                        throw new Exception("User was added but could not save the images");
                    }
                else
                acc.ImagePath = "/images/profile-pic.png";

               if (!isWebCall)
                    //Create the result that will be returned to the api called
                    return new RegisterResultApiModel
                    {
                        Email = registerdUser.Email,
                        Username = registerdUser.UserName,
                        FirstName = acc.FirstName,
                        LastName = acc.LastName,
                        ImagePath = acc.ImagePath,
                        //Token = registerdUser.GenerateJwtToken()
                    };*/
                #endregion
                await mContext.SaveChangesAsync();

                return result;
            }
            else
                //If task did not Succeeded return null
                return result.Errors;
        }
        public async Task<SignInResultUserDataApiModel> SignInUser(SignInUserDataApiModel userData)
        {
            //Check if the value provieded was an email
            var isEmail = userData.UserID.Contains('@');

            //Get the user with the sent data
            var account = await GetAccountByEmailOrUsername(userData.UserID);

            if (account == null)
                return null;

            //Check if the password is correct
            var isValidPassword = await mUserManger.CheckPasswordAsync(account.User, userData.Password);

            //If the password is not correct
            if (!isValidPassword)
                return null;

            //Get the user account
            var userAccount = await GetAccountByEmailOrUsername(userData.UserID);

            //Return the user information
            return new SignInResultUserDataApiModel
            {
                Username = account.User.UserName,
                FirstName = userAccount.FirstName,
                LastName = userAccount.LastName,
                Email = account.User.Email,
                Token = account.User.GenerateJwtToken(IoCCore.AppViewModel),
                ImagePath = account.ImagePath,
            };
        }
        public async Task<IEnumerable<ApplicationUser>> GetUsers()
        {
            try
            {
                //Try to get the users
                var users = await mContext.Users.ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task SignOut()
        {
            await mSignInManager.SignOutAsync();
        }
        public async Task<bool> UpdateUserInformation(string username, UpdateUserInformationViewModel userInfo)
        {
            var user = new ApplicationUser();
            try
            {
                //Get the user
                user = await mContext.Users.SingleOrDefaultAsync(item => item.UserName == username);
            }
            catch (Exception)
            {
                throw;
            }
            //Check if the user is found
            if (user == null)
                throw new NullReferenceException("The user is not found in please check the sent information");

            Account acc = mContext.Accounts.FirstOrDefault(x => x.ID == user.AccountID);
            acc.FirstName = userInfo.FirstName;
            acc.LastName = userInfo.LastName;
            acc.DateOfBirth = userInfo.DateOfBirth;
            acc.Gender = userInfo.Gender;
            
            user.PhoneNumber = userInfo.PhoneNumber;
            /*Tarek if (userInfo.Image != null)
            {
                if (acc.ImagePath != "/images/profile-pic.png")
                    //Delete the old image
                    FileHelpers.DeleteFile(acc.ImagePath, IoCCore.AppViewModel.ASPApplicaitonRootDirectory);

                acc.ImagePath = await FileHelpers.SaveImage(userInfo.Image, IoCCore.AppViewModel.ASPApplicaitonRootDirectory, FilePathsForWWWRoot.AccountImages, user.Account.ID);
            }*/

            //Mark it for update
            mContext.Entry(user).State = EntityState.Modified;
            //Save the changes
            await mContext.SaveChangesAsync();

            return true;
        }
        public async Task<object> GetAccountBaseData(string username)
        {
            try
            {
                var account = await mContext.Accounts.SingleOrDefaultAsync(item => item.User.UserName == username);


                return new {
                    account.ImagePath,
                    account.FirstName,
                    account.LastName,
                };

            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<object> GetUserInformation(string username)
        {
            var user = new ApplicationUser();
            try
            {
                //Get the user
                user = await mContext.Users.SingleOrDefaultAsync(item => item.UserName == username);
            }
            catch (Exception)
            {
                throw;
            }
            //Check if the user is found
            if (user == null)
                return null;
            Account acc = mContext.Accounts.FirstOrDefault(x => x.ID == user.AccountID);
            var infoToReturn = new
            {
                acc.FirstName,
                acc.LastName,
                acc.Gender,
                acc.DateOfBirth,
                acc.ImagePath,
                user.Email,
                user.PhoneNumber,
                user.UserName
            };

            return infoToReturn;
        }
        public async Task<string> ChangeUserfilePictureBase64String(string username, FileModel imgStr)
        {
            //Get the user account
            var user = await GetAccountByEmailOrUsername(username);

            /*Tarek if (user.ImagePath != "/images/profile-pic.png")
                //Delete the old image
                FileHelpers.DeleteFile(user.ImagePath, IoCCore.AppViewModel.ASPApplicaitonRootDirectory);

            //Save the new image
            user.ImagePath = await FileHelpers.SaveImage(imgStr, IoCCore.AppViewModel.ASPApplicaitonRootDirectory, FilePathsForWWWRoot.AccountImages, user.ID);
            */
            try
            {
                await mContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return user.ImagePath;
        }
        public async Task<string> ChangeUserProfilePictureIFromFileByteArray(string username, FileModel imgBytes)
        {
            //Get the user account
            var user = await GetAccountByEmailOrUsername(username);
            /*Tarek if (user.ImagePath != "images/profile-pic.png")
                //Delete the old image
                FileHelpers.DeleteFile(user.ImagePath, IoCCore.AppViewModel.ASPApplicaitonRootDirectory);

            //Save the new image
            user.ImagePath = await FileHelpers.SaveImage(imgBytes, IoCCore.AppViewModel.ASPApplicaitonRootDirectory, FilePathsForWWWRoot.AccountImages, user.ID);
            */
            try
            {
                await mContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return user.ImagePath;
        }
        public async Task<object> ChangeUserPassword(ChangePsswordViewModel model)
        {
            try
            {
                //Get the user
                var user = await mUserManger.FindByNameAsync(model.UserID);
                //Chamge the password
                var result = await mUserManger.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}


