using Ninject;
using NavaraAPI;
using SmartLifeLtd.ViewModels;

namespace NavaraAPI.Models
{
    public class ResetPasswordOrder
    {
        public string UserID { get; set; }
    }

    /// <summary>
    /// A class for the password change action parameter
    /// </summary>
    public class ForgettedPasswordChange
    {
        #region perties
        public string UserID { get; set; }
        /// <summary>
        /// The new user password
        /// </summary>
        public string NewPassword { get; set; }
        public string Token { set; get; }
        #endregion
    }
}
