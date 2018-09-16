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
    public static class StringExtention
    {
        public static string GetName(this string Name)
        {
            return Name.Split('@')?[0]?.Replace(".", " ");
        }


    }
}
