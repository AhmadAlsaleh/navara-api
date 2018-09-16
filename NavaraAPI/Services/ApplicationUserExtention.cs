using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using NavaraAPI.Enums;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Data.AspUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.Services
{
    public static class ApplicationUserExtention
    {
        public static async void SendVerificationCode(this ApplicationUser registerdUser, NumberTokenType TokenType, UserManager<ApplicationUser> userManager)
        {
            try
            {
                string token = "";
                switch(TokenType)
                {
                    case NumberTokenType.PhoneNumberToken:
                        token = await userManager.GenerateChangePhoneNumberTokenAsync(registerdUser, registerdUser.PhoneNumber);
                        break;
                    default:
                        throw new System.NotImplementedException("No implement for Token type:" + TokenType.ToString());
                }
                if (registerdUser.UserName.IsValidEmail())
                   // EmailService.SendConfirmationNumer(registerdUser.UserName, token);
                    registerdUser.EmailConfirmed = true;
                else if (registerdUser.UserName.IsValidPhone())
                    SMSService.SendConfirmSMS(registerdUser.UserName, token);
                else if (!string.IsNullOrWhiteSpace(registerdUser.UserName))
                {
                }
            }
            catch
            {
                throw;
            }
        }


        public static async void SendVerificationURL(this ApplicationUser registerdUser, URLTokenType TokenType, UserManager<ApplicationUser> userManager)
        {
            try
            {
                string token = "";
                switch (TokenType)
                {
                    case URLTokenType.PhoneNumberToken:
                        token = await userManager.GenerateChangePhoneNumberTokenAsync(registerdUser, registerdUser.PhoneNumber);
                        break;
                    case URLTokenType.EmailToken:
                        token = await userManager.GenerateChangeEmailTokenAsync(registerdUser, registerdUser.Email);
                        break;
                }
                if (registerdUser.UserName.IsValidEmail())
                    EmailService.SendConfirmationEmail(registerdUser.UserName, registerdUser.Id, token);
                else if (registerdUser.UserName.IsValidPhone())
                    SMSService.SendConfirmSMS(registerdUser.UserName, token);
                else if (!string.IsNullOrWhiteSpace(registerdUser.UserName))
                {
                }
            }
            catch
            {
                await userManager.DeleteAsync(registerdUser);
                throw;
            }
        }
    }
}
