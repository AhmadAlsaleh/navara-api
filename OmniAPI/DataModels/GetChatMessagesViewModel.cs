using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Models
{
    public class GetChatMessagesViewModel
    {
        public Guid? UserID { get; set; }
        public Guid? ChatID { get; set; }
    }
}
