using SmartLifeLtd.Data;
using SmartLifeLtd.Data.DataContexts;
using System;
using System.ComponentModel.DataAnnotations;
using SmartLifeLtd.Data.Tables;
using Microsoft.AspNetCore.Http;
using SmartLifeLtd.Data.Tables.Omni;

namespace OmniAPI.Models
{
    public class MessageDataModel
    {
        public Guid? AdID { set; get; }
        public string Title { set; get; }
        public string SenderName { set; get; }
        public string Email { set; get; }
        public string Body { set; get; }
        public string Attachment { set; get; }
    }
}
