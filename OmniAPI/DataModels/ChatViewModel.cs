using SmartLifeLtd.Data.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace OmniAPI.Models
{
    public class ChatDataModel
    {
        public Guid? ChatID { set; get; }
        public Guid? AdID { set; get; }
        public string Owner { set; get; }
        public string AdOwner { set; get; }
        public Guid? OwnerAccountID { set; get; }
        public string AdTitle { set; get; }
        public string Image { set; get; }
        public List<ChatMessageDataModel> Messages { set; get; }
    }

    public class ChatMessageDataModel
    {
        public Guid? ReceiverID { set; get; }
        public string Title { set; get; }
        public string Body { set; get; }
        public DateTime? SentDate { set; get; }
        public DateTime? SeenDate { set; get; }
        public string SenderName { set; get; }
        public string ImagePath { get; set; }
    }


    public class GetChatMessageDataModel
    {
        public int Page { set; get; } = 0;
        public int Count { set; get; } = 10;
        public Guid? ChatID { set; get; }
    }
}
