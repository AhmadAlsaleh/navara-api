using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using SmartLifeLtd.Data.AspUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.Services
{
    public interface IEmailService
    {
    }

    public  class EmailService : IEmailService
    {
        public static string AppName { set; get; } = "Navara Store";
        public static string SenderName { set; get; } = "Navara Store";
        public static string SenderEmail { set; get; } = "noreply@navarastore.com";
        public static string Password { set; get; } = "P@ssw0rd";
        public static string ConfirmURL { set; get; } = "http://Api.NavaraStore.com/User/ConfirmEmail/";
        public static string ResetURL { set; get; } = "http://Api.NavaraStore.com/Users/ResetPassword?token={0}&UserID={1}";
        public static string ConfirmationURL { set; get; } = "http://Api.NavaraStore.com/Users/Confirm?token={0}&UserID={1}";
        public static int ServerPort { set; get; } = 25;
        public static string ServerMailHost { set; get; } = "navarastore.com";
        public static bool SSLNeed { set; get; } = false;
        public EmailService(UserManager<ApplicationUser> userManager)
        {

        }

        public static async void SendConfirmationNumer(string toEmail, string Token)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(SenderName, SenderEmail));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = $"{AppName} confirm email";
                string text = $"Thanks for joining us in {AppName} family, Your confirmation number is: {Token}\n";
                message.Body = new TextPart("plain") { Text = text };
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(
                       ServerMailHost,
                       ServerPort,
                       SSLNeed)
                       .ConfigureAwait(false);
                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    // Note: only needed if the SMTP server requires authentication
                    await client.AuthenticateAsync(SenderEmail, Password)
                            .ConfigureAwait(false);
                    await client.SendAsync(message).ConfigureAwait(false);
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public static void SendConfirmationEmail(string toEmail, string UserID, string Token)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(SenderName, SenderEmail));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = $"{AppName} confirm email";
                string text = $"Your can Reset your Password using this URL: {String.Format(ResetURL, Token, UserID)}";
                message.Body = new TextPart("plain") { Text = text };
                using (var client = new SmtpClient())
                {
                    client.Connect(ServerMailHost, ServerPort, SSLNeed);
                    client.Authenticate(SenderEmail, Password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void SendResetEmail(string toEmail, string UserID, string Token)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(SenderName, SenderEmail));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = "Navara Store Reset Password";
                string text = $"Your can Reset your Password using this URL: {String.Format(ResetURL, Token, UserID)}";
                message.Body = new TextPart("plain") { Text = text };
                using (var client = new SmtpClient())
                {
                    client.Connect(ServerMailHost, ServerPort, SSLNeed);
                    client.Authenticate(SenderEmail, Password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void SendEmail(string username, string email, string text, string subject)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(SenderName, SenderEmail));
                message.To.Add(new MailboxAddress(username, SenderEmail));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = text };
                using (var client = new SmtpClient())
                {
                    client.Connect(ServerMailHost, ServerPort, SSLNeed);
                    client.Authenticate(SenderEmail, Password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
