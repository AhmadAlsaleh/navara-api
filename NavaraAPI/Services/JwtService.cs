using MailKit.Net.Smtp;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using SmartLifeLtd.Data.AspUsers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NavaraAPI.Services
{
    public class JwtService
    {
        #region Jwt token variables
        public static string Issuer { get; set; }
        public static string Audience { get; set; }
        public static string SecretKey { get; set; }
        #endregion

        public static string GenerateJwtToken(ApplicationUser user)
        {
            //Set the claims for the token
            var claims = new[]
            {
                //Set a unique key for the clam
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                //Set the username for use in the httpcontext
                new Claim(ClaimsIdentity.DefaultNameClaimType,user.UserName),
            };
            //Create the credentials that are used for the token
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
                SecurityAlgorithms.HmacSha256
                );

            //Create the jwt token
            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.Now.AddMonths(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GenerateJwtToken2(ApplicationUser user)
        {
            //Set the claims for the token
            var claims = new[]
            {
                //Set a unique key for the clam
                new Claim(ClaimTypes.MobilePhone,Guid.NewGuid().ToString()),
                //Set the username for use in the httpcontext
                new Claim(ClaimsIdentity.DefaultNameClaimType,user.PhoneNumber),
            };
            //Create the credentials that are used for the token
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
                SecurityAlgorithms.HmacSha256
                );

            //Create the jwt token
            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.Now.AddMonths(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
