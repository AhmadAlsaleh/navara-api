using SmartLifeLtd.Data;
using SmartLifeLtd.Data.DataContexts;
using System;
using System.ComponentModel.DataAnnotations;
using SmartLifeLtd.Data.Tables;
using Microsoft.AspNetCore.Http;
using SmartLifeLtd.Data.Tables.Omni;

namespace OmniAPI.Models
{
    public class MessageViewModel
    {
        public MessageViewModel()
        {
            ad = new AD();
      
        }

        public AD ad { set; get; }
        public Guid? AdID { set; get; }
        public Guid? SenderID { set; get; }
        public Guid ID { set; get; }
        public string Title { set; get; }
        public string SenderName { set; get; }
        [DataType(DataType.EmailAddress)]
        public string Email { set; get; }
        public string Body { set; get; }
        public Guid? RecieverID { set; get; }
    
        public DateTime SendDate { get; set; }

        public string Attachment { set; get; }
    }
}
