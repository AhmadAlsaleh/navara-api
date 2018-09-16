using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
using SmartLifeLtd;
using SmartLifeLtd.Management.Interfaces;
using SmartLifeLtd.Data.Tables.Shared;
using SmartLifeLtd.Classes;
using NavaraAPI.IServices;

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
        private readonly UserManager<ApplicationUser> mUserManager;
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
            mUserManager = userManager;
        }
        #endregion

        public async Task<IAccount> GetAccountByID(Guid id)
        {
            return await mContext.Set<Account>().Include(item => item.User).SingleOrDefaultAsync(ac => ac.ID == id);
        }
        public async Task<IAccount> GetAccountByUserID(string UserID)
        {
            try
            {
                var account = (await mContext.Set<Account>().Include(user => user.User)
                    .Where(x => x.User != null)
                    .SingleOrDefaultAsync(user => user.User.Email == UserID || user.User.UserName == UserID));
                return account;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<object> Register(RegisterUserDataModel userData)
        {
            var newUser = new ApplicationUser
            {
                UserName = userData.UserID,
                Email = userData.Email,
                PhoneNumber = userData.PhoneNumber,
                TwoFactorEnabled = false,
                PhoneNumberConfirmed = false,
                EmailConfirmed = false,
                LockoutEnabled = false
            };
            var result = await mUserManager.CreateAsync(newUser, userData.Password);

            
            if (result.Succeeded)
            {
                var registerdUser = await mUserManager.FindByNameAsync(userData.UserID);

                try
                {
                    await mSignInManager.SignInAsync(registerdUser, true);
                    registerdUser.SendVerificationCode(Enums.NumberTokenType.PhoneNumberToken, mUserManager);
                    var userToken = JwtService.GenerateJwtToken(registerdUser);

                    Account acc = new Account
                    {
                        Name = string.IsNullOrWhiteSpace(userData.FirstName) ? userData.Email.GetName() : userData.FirstName,
                        CreationDate = DateTime.UtcNow,
                        CashBack = 0,
                        Mobile = userData.PhoneNumber,
                        LanguageID = this.mContext.Set<Language>().FirstOrDefault(x => x.IsDefault == true)?.ID
                    };
                    mContext.Set<Account>().Add(acc);
                    await mContext.SaveChangesAsync();
                    //------------------------------------------------
                    registerdUser.AccountID = acc.ID;
                    acc.UserID = registerdUser.Id;
                    await mContext.SaveChangesAsync();
                    //------------------------------------------------

                    return userToken;
                }
                catch
                { 
                    if(registerdUser.AccountID != null)
                    {
                        Account acc = mContext.Set<Account>().SingleOrDefault(x => x.ID == registerdUser.AccountID);
                        if (acc != null) mContext.Set<Account>().Remove(acc);
                    }
                    ApplicationUser user = mContext.Set<ApplicationUser>().SingleOrDefault(x => x.Id == registerdUser.Id);
                    if (user != null) mContext.Set<ApplicationUser>().Remove(user);
                    await mContext.SaveChangesAsync();
                    throw;
                }
            }
            else
                return result.Errors;
        }

      

        public async Task<object> SignIn(SignInUserDataApiModel userData)
        {
            try
            {
                var registerdUser = await mUserManager.FindByNameAsync(userData.UserID);
                if (registerdUser == null) return null;

                var isValidPassword = await mUserManager.CheckPasswordAsync(registerdUser, userData.Password);
                if (!isValidPassword) throw new Exception("Wrong Password");
                await mSignInManager.SignInAsync(registerdUser, true);
                var userToken = JwtService.GenerateJwtToken(registerdUser);
                return userToken;
            }
            catch
            {
                throw;
            }
        }

        public async Task SignOut()
        {
            await mSignInManager.SignOutAsync();
        }

        public async Task<object> UpdateUserInformation(string userID, UpdateUserInformationViewModel model)
        {
            try
            {
                ApplicationUser user = await mContext.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = mContext.Set<Account>().FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                account.Name = model.FirstName;
                account.Mobile = model.PhoneNumber;
                return true;
            }
            catch { return false; }
        }
        public async Task<object> GetUserInformation(string userID)
        {
            try
            {
                ApplicationUser user = await mContext.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = mContext.Set<Account>().FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                var infoToReturn = new
                {
                    account.Name,
                    account.Mobile,
                    account.CartID,
                    account.CashBack,
                    account.LanguageID,
                    user.Email,
                    user.UserName,
                    IsVerified = user.PhoneNumberConfirmed || user.EmailConfirmed
                };
                return infoToReturn;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> ChangePassword(string UserID, string OldPassword, string NewPassword)
        {
            try
            {
                var user = await mUserManager.FindByNameAsync(UserID);
                var result = await mUserManager.ChangePasswordAsync(user, OldPassword, NewPassword);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ConfirmAccount(string userID, string Token)
        {
            try
            {
                ApplicationUser user = await mContext.Users.SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = mContext.Set<Account>().FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return false;
                bool result = false;
                result = await mUserManager.VerifyChangePhoneNumberTokenAsync(user, Token, user.PhoneNumber);
                if (result == true)
                {
                    user.PhoneNumberConfirmed = true;
                    await mContext.SaveChangesAsync();
                    return true;
                }
                var identityResult = await mUserManager.ConfirmEmailAsync(user, Token);
                if(identityResult.Succeeded)
                {
                    user.EmailConfirmed = true;
                    await mContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> ChangeUserfilePictureBase64String(string username, FileModel imgStr)
        {
            return null;
        }
        public async Task<string> ChangeUserProfilePictureIFromFileByteArray(string username, FileModel imgBytes)
        {
            return null;
        }
    }
}


