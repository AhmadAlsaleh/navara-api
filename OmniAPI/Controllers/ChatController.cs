using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using OmniAPI.Models;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Omni;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OmniAPI.Controllers.API
{
    [Route("[controller]/[Action]")]
    public class ChatController : Controller
    {
        private readonly OmniDbContext _context;

        public ChatController(OmniDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [AuthorizeToken]
        public async Task<IActionResult> GetChatMessages([FromBody] GetChatMessageDataModel model)
        {
            try
            {
                #region Check user
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = _context.Set<Account>()
                    .Include(x => x.Chats)
                        .ThenInclude(x => x.AD)
                            .ThenInclude(x => x.ADImages)
                    .Include(x => x.Chats)
                        .ThenInclude(x => x.Messages)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                #endregion

                var chat = account.Chats.FirstOrDefault(x => x.ID == model.ChatID);
                if (chat == null) return BadRequest("Chat ID is not related to any Chat");
                var messages = chat.Messages.OrderBy(x => x.SentDate).Select(msg => new ChatMessageDataModel()
                {
                    Body = msg.Body,
                    SeenDate = msg.SeenDate,
                    ReceiverID = msg.ReceiverAccountID,
                    SenderName = msg.SenderName,
                    SentDate = msg.SentDate,
                    Title = msg.Title,
                    ImagePath = msg.FilePath
                }).Skip(model.Page * model.Count).Take(model.Count).ToList();
                chat.Messages.Where(x => x.SeenDate == null).ForEach(x => x.SeenDate = DateTime.UtcNow);
                await _context.SubmitAsync();
                return Json(messages);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AuthorizeToken]
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody]MessageDataModel model)
        {
            #region Check user
            var userID = HttpContext.User.Identity.Name;
            if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
            Account account = _context.Set<Account>()
                .Include(x => x.Chats)
                    .ThenInclude(x => x.AD)
                        .ThenInclude(x => x.ADImages)
                .Include(x => x.Chats)
                    .ThenInclude(x => x.Messages)
                .FirstOrDefault(x => x.ID == user.AccountID);
            if (user == null || account == null) return null;
            #endregion

            AD ad = _context.ADs.SingleOrDefault(x => x.ID == model.AdID);
            if (ad == null) return BadRequest("Ad not exist");
            try
            {
                var chat = _context.Chats.SingleOrDefault(x => x.ADID == model.AdID && x.OwnerAccountID == account.ID);
                if (chat == null)
                {
                    chat = new Chat()
                    {
                        ADID = model.AdID,
                        Messages = new System.Collections.ObjectModel.ObservableCollection<Message>(),
                        OwnerAccountID = account.ID,
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
                    SenderAccountID = account.ID,
                    ReceiverAccountID = ad.AccountID,
                    SenderName = model.SenderName,
                    ChatID = chat.ID,
                    SeenDate = null
                };
                _context.Messages.Add(message);

                if (model.Attachment != null || model.Attachment.Length != 0)
                {
                    string path = ImageOperations.SaveImage(model.Attachment, message);
                    message.FilePath = path;
                }
                await _context.SubmitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AuthorizeToken]
        [HttpGet("{ID}")]
        public async Task<IActionResult> DeleteChat(Guid ID)
        {
            try
            {
                #region Check user
                var userID = HttpContext.User.Identity.Name;
                if (userID == null) return StatusCode(StatusCodes.Status401Unauthorized);
                ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(item => item.UserName == userID);
                Account account = _context.Set<Account>()
                    .Include(x => x.Chats)
                        .ThenInclude(x => x.Messages)
                    .FirstOrDefault(x => x.ID == user.AccountID);
                if (user == null || account == null) return null;
                #endregion

                var chat = account.Chats.SingleOrDefault(x => x.ID == ID);
                if (chat == null)
                    return BadRequest("ID is not relatred to any chat");
                _context.Messages.RemoveRange(chat.Messages);
                _context.Chats.Remove(chat);
                await _context.SubmitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
