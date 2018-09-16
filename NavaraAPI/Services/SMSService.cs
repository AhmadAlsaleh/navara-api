using MailKit.Net.Smtp;
using MimeKit;
using System;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;

namespace NavaraAPI.Services
{
    public static class SMSService
    {
        public static string AccountSID { set; get; } = "ACf661e4155436aabeeb3c3f2240cc0d95";
        public static string AUTHTOKEN { set; get; } = "f7fdd5816d2fcd57a047bc0ff078e178";
        public static string MOBILE { set; get; } = "+12085444049";

        public static bool SendConfirmSMS(string toNumber, string Token)
        {
            try
            {
                TwilioClient.Init(AccountSID, AUTHTOKEN);

                var to = new PhoneNumber(toNumber);
                var message = MessageResource.Create(
                    to,
                    from: new PhoneNumber(MOBILE), //  From number, must be an SMS-enabled Twilio number ( This will send sms from ur "To" numbers ).
                    body: $"Thank you for joining our Store, your verification number is {Token}");
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static bool SendResetSMS(string toNumber, string UserID, string Token)
        {
            try
            {
                TwilioClient.Init(AccountSID, AUTHTOKEN);

                var to = new PhoneNumber(toNumber);
                var message = MessageResource.Create(
                    to,
                    from: new PhoneNumber(MOBILE), //  From number, must be an SMS-enabled Twilio number ( This will send sms from ur "To" numbers ).
                    body: $"You can reset your password using this URL: {String.Format(EmailService.ResetURL, Token, UserID)}");
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
