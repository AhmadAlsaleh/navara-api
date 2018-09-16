using NavaraAPI.Models;
using SmartLifeLtd;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Management.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NavaraAPI.IServices
{
    /// <summary>
    /// The users servce to work with the <see cref="Account"/> Table
    /// </summary>
    public interface IUsersService
    {
        /// <summary>
        /// Registers a user in the database 
        /// and return the user with his token
        /// </summary>
        /// <param name="userdate">The user data that is sent from the client to the server</param>
        /// <returns></returns>
        Task<object> Register(RegisterUserDataModel userdate);

        /// <summary>
        /// Signs in a user and sends back a token 
        /// </summary>
        /// <param name="userData">The user data that was sent to the server</param>
        /// <returns></returns>
        /// 
        Task<object> SignIn(SignInUserDataApiModel userData);

        /// <summary>
        /// Gets the account with the respected guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IAccount> GetAccountByID(Guid id);

        /// <summary>
        /// Gets the account of the user with the respected email or username
        /// </summary>
        /// <param name="emailOrUsername">Either the email or the username of the account</param>
        /// <returns></returns>
        Task<IAccount> GetAccountByUserID(string userID);

        /// <summary>
        /// Signs a user out of the application
        /// for cookie based but not for Jwt token as it will 
        /// still be valid even after sign out
        /// </summary>
        /// <returns></returns>
        Task SignOut();

        /// <summary>
        /// Gets all the user information in the database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<object> GetUserInformation(string userID);

        /// <summary>
        /// Update user information in the database
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        Task<object> UpdateUserInformation(string userID, UpdateUserInformationViewModel model);

        /// <summary>
        /// Deletes the old profile picture and sets a new one
        /// </summary>
        /// <param name="username">The user to upddate for</param>
        /// <param name="imgStr">The image base64 string</param>
        /// <returns></returns>
        Task<string> ChangeUserfilePictureBase64String(string userID, FileModel imgStr);

        /// <summary>
        /// Deketes the old profile picture if found and sets the the new sent one
        /// </summary>
        /// <param name="username"></param>
        /// <param name="imgBytes"></param>
        /// <returns></returns>
        Task<string> ChangeUserProfilePictureIFromFileByteArray(string userID, FileModel imgBytes);

        /// <summary>
        /// Changes the user password in the database
        /// </summary>
        /// <returns></returns>
        Task<object> ChangePassword(string UserID, string OldPassword, string NewPassword);

        Task<bool> ConfirmAccount(string userID, string Token);
    }
}
