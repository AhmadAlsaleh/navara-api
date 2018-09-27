using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OmniAPI.Models;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Omni;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Controllers.API
{
    [Route("api/[controller]/[Action]")]
    public class ChatAPIController : Controller
    {
        private readonly OmniDbContext _context;
        private readonly LogDbContext _logContext;

        public ChatAPIController(OmniDbContext context, LogDbContext LogDbContext)
        {
            _context = context;
            _logContext = LogDbContext;

        }
        [HttpGet("{AccountID}")]
        public IActionResult GetChatNotSeen(Guid AccountID)
        {
            try
            {
                var account = _context.Accounts.Include("Chats")
                            .SingleOrDefault(x => x.ID == AccountID);
                if (account == null) return BadRequest("Account Token is not related to any account");
                var allChats = _context.Chats.Include("Ad").Include("Messages")
                               .Where(chat =>
                               (chat.AD.AccountID == account.ID || chat.OwnerAccountID == account.ID)
                               && chat.Messages.FirstOrDefault(s => s.SeenDate == null) != null)
                               .Select(s => s.ID).ToList();
                //var allChatss = _context.Chats.Include("Ad").Include("Messages")
                //              .Where(chat => (chat.Ad.AccountID == account.ID || chat.OwnerAccountID == account.ID))
                //              .Select(s => s.Messages).ToList();
                if (allChats == null || allChats.Count == 0)
                    return Ok("[]");
                else return Ok( allChats );
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
            }

        /*[HttpGet("{Token}")]
        public async Task<IActionResult> GetAccountChat(Guid Token)
        {
            var chat = _context.Chats.Include("AccountTokens").Include("ReceivedMessages").Include("SentMessages")
                            .Where(x => x.AccountTokens.Any(y => y.Token == Token.ToString()))
                            .SingleOrDefault();
            if (chat == null)
                return BadRequest("Account Token is not relatred to any account");
            List<ChatViewModel> messages = chat.ReceivedMessages.Select(x => new ChatViewModel()
            {
                ChatID = x.c,
                Body = x.Body,
                SeenDate = x.SeenDate,
                SenderAccountID = x.SenderAccountID,
                SenderName = x.SenderName,
                SentDate = x.SentDate,
                Title = x.Title
            }).ToList();
            messages.AddRange(chat.SentMessages.Select(x => new ChatViewModel()
            {
                Body = x.Body,
                SeenDate = x.SeenDate,
                SenderAccountID = x.SenderAccountID,
                SenderName = x.SenderName,
                SentDate = x.SentDate,
                Title = x.Title
            }));
            return Json(messages);
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetAdChat(Guid ID)
        {
            var item = _context.Messages.Where(x => x.AdID == ID);
            List<ChatViewModel> messages = item.Select(x => new ChatViewModel()
            {
                Body = x.Body,
                SeenDate = x.SeenDate,
                SenderAccountID = x.SenderAccountID,
                SenderName = x.SenderName,
                SentDate = x.SentDate,
                Title = x.Title
            }).ToList();
            return Json(messages);
        }*/

        [HttpGet("{Token}")]
        public async Task<IActionResult> GetChats(Guid Token)
        {
            try
            {
                var account = _context.Accounts.Include("AccountTokens").Include("Chats")
                            .Where(x => x.AccountTokens.Any(y => y.Token == Token.ToString()))
                            .SingleOrDefault();
                if (account == null) return BadRequest("Account Token is not related to any account");
                var allChats = _context.Chats.Include("Ad").Include("Ad.AdImages").Include("Messages")
                               .Where(chat => chat.AD.AccountID == account.ID || chat.OwnerAccountID == account.ID).ToList();
                if (allChats == null || allChats.Count == 0)
                    return Ok("[]");
                var chats = (from chat in allChats
                             select new ChatViewModel()
                             {
                                 ChatID = chat.ID,
                                 AdID = chat.ADID,
                                 Owner = chat.OwnerName,
                                 OwnerAccountID = chat.OwnerAccountID,
                                 AdTitle = chat.AD?.Title,
                                 Image = chat.AD?.ADImages?.SingleOrDefault(S => S.IsMain == true).ImagePath ?? "",
                                 Messages = new List<ChatMessageViewModel>()
                                 {
                                    new ChatMessageViewModel() {
                                        Body = chat.Messages.OrderBy(x => x.SentDate).LastOrDefault()?.Body,
                                        SeenDate = chat.Messages.OrderBy(x => x.SentDate).LastOrDefault()?.SeenDate,
                                        SenderAccountID = chat.Messages.OrderBy(x => x.SentDate).LastOrDefault()?.SenderAccountID,
                                        SenderName = chat.Messages.OrderBy(x => x.SentDate).LastOrDefault()?.SenderName,
                                        SentDate = chat.Messages.OrderBy(x => x.SentDate).LastOrDefault()?.SentDate,
                                        Title = chat.Messages.OrderBy(x => x.SentDate).LastOrDefault()?.Title }
                                 }
                             }).ToList();
                foreach (var chat in chats)
                {
                   
                    AD ad = _context.ADs.Include("AdImages").SingleOrDefault(x => x.ID == chat.AdID);
                    if (ad == null) return BadRequest("No Ad related to chat");
                    ADImage adImage = ad.ADImages.FirstOrDefault(x => x.IsMain == true);
                    /*string filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\No-image-found.jpg";
                    if (adImage != null)
                    {
                        filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\ItemImages\\{ad.Code}\\{adImage.ImagePath}";
                        if (!System.IO.File.Exists(filePath))
                            filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\No-image-found.jpg";
                    }
                    chat.AdImage = System.IO.File.ReadAllBytes(filePath);*/
                
                    if (adImage == null)
                    {
                        chat.Image  = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                        //if (!System.IO.File.Exists(filePath))
                        //    filePath = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                    }
              
                }
                return Json(chats);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetChatMessages([FromBody]GetChatMessagesViewModel model)
        {
            try
            {
                var chat = _context.Chats.Include("Messages").Include("Ad").Include("Ad.AdImages").Include("OwnerAccount").SingleOrDefault(x => x.ID == model.ChatID);
                if (chat == null) return BadRequest();
                ChatViewModel chatviewmodel = new ChatViewModel()
                {
                    IsDisabled = chat.AD.IsDisabled ?? false,
                    AdID = chat.ADID,
                    AdTitle = chat.AD?.Title,
                    ChatID = chat.ID,
                    Owner = chat.OwnerName,
                    AdOwner = chat.OwnerAccount.Name,
                    OwnerAccountID = chat.OwnerAccountID,
                    Image = chat.AD?.ADImages?.SingleOrDefault(S => S.IsMain == true).ImagePath ?? "",
                    Messages = new List<ChatMessageViewModel>()
                };
                ADImage adImage = chat.AD.ADImages.FirstOrDefault(x => x.IsMain == true);
                /*string filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\No-image-found.jpg";
                if (adImage != null)
                {
                    filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\ItemImages\\{chat.Ad.Code}\\{adImage.ImagePath}";
                    if (!System.IO.File.Exists(filePath))
                        filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\No-image-found.jpg";
                }
                chatviewmodel.AdImage = System.IO.File.ReadAllBytes(filePath);*/

                //string filePath = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                if (adImage == null)
                {
                    chatviewmodel.Image = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                    //if (!System.IO.File.Exists(filePath))
                    //    filePath = $"{this.HttpContext.Request.Host.Value}/images/No-image-found.jpg";
                }
                if (chat.Messages != null)
                {
                    foreach (var msg in chat.Messages.OrderBy(x => x.SentDate))
                    {
                        chatviewmodel.Messages.Add(new ChatMessageViewModel()
                        {
                            Body = msg.Body,
                            SeenDate = msg.SeenDate,
                            SenderAccountID = msg.SenderAccountID,
                            SenderName = msg.SenderName,
                            SentDate = msg.SentDate,
                            Title = msg.Title,
                            ImagePath=msg.FilePath

                        });
                    }
                    var ReciverUSer = _context?.Accounts?.SingleOrDefault(s => s.ID == model.UserID);
               
                    foreach (var message in chat.Messages.Where(x => x.SeenDate == null))
                    {
                        if(message.Chat.OwnerAccountID!=ReciverUSer.ID)
                        message.SeenDate = DateTime.UtcNow;
                    }
                        
                    _context.SubmitAsync();
                }
                return Json(chatviewmodel);
            }
            catch(Exception ex)
            { return BadRequest(ex.Message); }
        }

        /*[HttpGet]
        public async Task<IActionResult> GetChatWithAccountOnAd(ChatParameterViewModel parameter)
        {
            var account = _context.Accounts.Include("AccountTokens").Include("Chats")
                            .Where(x => x.AccountTokens.Any(y => y.Token == parameter.Token.ToString()))
                            .SingleOrDefault();
            if (account == null) return BadRequest("Account Token is not relatred to any account");

            var chat = account.Chats.SingleOrDefault(x => x.ID == parameter.ChatID);
            var messages = chat.Messages.Select(msg =>
                            new ChatViewModel()
                            {
                                ChatID = msg.ChatID,
                                Body = msg.Body,
                                SeenDate = msg.SeenDate,
                                SenderAccountID = msg.SenderAccountID,
                                SenderName = msg.SenderName,
                                SentDate = msg.SentDate,
                                Title = msg.Title,
                                AdTitle = msg.Chat.Ad.Title
                            });
            return Json(messages);
        }*/
        public string SaveImage(string ImgStr, Message newAd)
        {
                                                                               
            String path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images\\Message\\" + newAd.ID);

            //Check if directory exist
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }

            string imageName = Guid.NewGuid() + ".jpg";

            //set the image path
            string imgPath = Path.Combine(path, imageName);

            byte[] imageBytes = Convert.FromBase64String(ImgStr);

            System.IO.File.WriteAllBytes(imgPath, imageBytes);
            return imageName;
        }
      
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody]MessageViewModel model)
        {
          
            AD ad = _context.ADs.SingleOrDefault(x => x.ID == model.AdID);
            if (ad == null) return BadRequest("Ad not exist");
            try
            {
                var chat = _context.Chats.SingleOrDefault(x => x.ID == model.ID);
                if (chat == null)
                {
                    chat = new Chat()
                    {
                        ADID = model.AdID,
                        Messages = new System.Collections.ObjectModel.ObservableCollection<Message>(),
                        OwnerAccountID = model.SenderID,
                        OwnerName = model.SenderName,
                        StartDate = DateTime.UtcNow
                    };
                    _context.Chats.Add(chat);
                }
                Message message = new Message()
                {
                    Body = model.Body,
                    Title = model.Title,
                    SentDate = DateTime.UtcNow,
                    SenderAccountID = model.SenderID,
                    ReceiverAccountID = model.RecieverID ?? ad.AccountID,
                    SenderName = model.SenderName,
                    ChatID = chat.ID,
                    SeenDate = null
               
                };
                _context.Messages.Add(message);

                if (model.Attachment != null || model.Attachment.Length != 0)
                {
                    try
                    {
                        if (model.Attachment.Length > 0)
                        {
                            var fileName = SaveImage(model.Attachment, message);
                            message.FilePath = "images\\Message\\" + message.ID + "\\" + fileName;
                           
                        }

                        //var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Message/" + message.ID + "/" + Guid.NewGuid().ToString() + ".jpg");
                        //var dir = Path.GetDirectoryName(path);

                        //    string clientKey = this.Request.Headers["ClientKey"];
                        //    Directory.CreateDirectory(dir);
                        //    if (System.IO.File.Exists(path))
                        //        System.IO.File.Delete(path);
                        //    using (var stream = new FileStream(path, FileMode.Create))
                        //    {
                        //        await model.Attachment.CopyToAsync(stream);
                        //        /*_logContext.LogDbOperations.Add(new SmartLifeLtd.Data.Tables.Log.LogDbOperation()
                        //        {
                        //            Client = clientKey,
                        //            CreationDate = DateTime.Now,
                        //            OperationType = OperationType.AddFile.ToString(),
                        //            Remark = model.Attachment.FileName
                        //        });*/
                        _context.SubmitAsync();
                        return Ok();
                  
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { Error = ex.Message });
                    }
                }

                _context.SubmitAsync();
                //_logContext.SubmitAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{ID}")]
        public async Task<IActionResult> DeleteChat(Guid ID)
        {
            try
            {
                var chat = _context.Chats.Include("Messages").SingleOrDefault(x => x.ID == ID);
                if (chat == null)
                    return BadRequest("ID is not relatred to any account");
                _context.Messages.RemoveRange(chat.Messages);
                _context.Chats.Remove(chat);
                _context.SubmitAsync();
                //Tarek await chat.Remove(_context, true);
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
