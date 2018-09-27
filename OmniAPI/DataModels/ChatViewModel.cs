using SmartLifeLtd.Data.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace OmniAPI.Models
{
    public class ChatViewModel
    {
        public Guid? ChatID { set; get; }
        public Guid? AdID { set; get; }
        public bool IsDisabled { get; set; }
        public string Owner { set; get; }
        public string AdOwner { set; get; }
        public Guid? OwnerAccountID { set; get; }
        public string AdTitle { set; get; }
        public string Image { set; get; }
        public List<ChatMessageViewModel> Messages { set; get; }
    }

    public class ChatMessageViewModel
    {
        public Guid? SenderAccountID { set; get; }
        public string Title { set; get; }
        public string Body { set; get; }
        public DateTime? SentDate { set; get; }
        public DateTime? SeenDate { set; get; }
        public string SenderName { set; get; }
        public string ImagePath { get; set; }
    }
}
