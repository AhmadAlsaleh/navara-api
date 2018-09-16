using Ninject;
using NavaraAPI;
using SmartLifeLtd.ViewModels;

namespace NavaraAPI.Models
{
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
}
