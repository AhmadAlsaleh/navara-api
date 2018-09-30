using Microsoft.AspNetCore.Http;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Data.Tables.Omni;
using SmartLifeLtd.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace OmniAPI.Classes
{
    public static class AdExtention
    {
        public static string GetMainImageRelativePath(this AD ad)
        {
            string mainImage = ad.ADImages.FirstOrDefault(x => x.IsMain == true)?.ImagePath;
            if (string.IsNullOrWhiteSpace(mainImage) || !File.Exists(mainImage.GetFilePathOnServer()))
                mainImage = $"images/No-image-found.jpg";
            return mainImage;
        }
    }
}
