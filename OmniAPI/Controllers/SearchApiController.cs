using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Omni;
using SmartLifeLtd.Classes;
using OmniAPI.Models;

namespace OmniWeb.Controllers.Mobile
{
    [Route("api/[controller]/[Action]")]
    public class SearchAPIController : Controller
    {
        private readonly OmniDbContext _context;
        private readonly LogDbContext _logContext;
        public SearchAPIController(OmniDbContext context, LogDbContext LogDbContext)
        {
            _context = context;
            _logContext = LogDbContext;
        }
        [HttpGet("{ID}")]
        public IActionResult GetSearch(Guid? ID)
        {
            try
            {
                var Searches = _context?.Searches?.Where(s => s.AccountID == ID).Select(
                   s=>new {SearchWord=s.Keywords})?.ToList();
                if (Searches?.Count > 0)
                {
                    return Ok(Searches);
                }
                else
                {
                    return Ok("[]");
                }
            }
            catch (Exception e)
            {
                BadRequest(e.Message);
               
            }
            return BadRequest();
        }
        [HttpPost]
        public IActionResult SetSearch([FromBody] SearchAPIViewModel model)
        {
            try
            {
                var Account = _context?.Accounts?.SingleOrDefault(C => C.ID == model.UserID);
                var searches = _context?.Searches?.Where(s => s.AccountID == Account.ID && s.Keywords.ToLower() == model.SearchWord.ToLower()).ToList();
                if (searches.Count <= 0)
                {
                    Search NewSearch = new Search
                    {
                        Account = Account,
                        Keywords = model.SearchWord,
                        CreationDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow,

                    };
                    _context.Searches.Add(NewSearch);
                    _context.SubmitAsync();
                }
  
                return Ok();
            }
            catch (Exception e )
            {
                return BadRequest();
         
            }
        }
    }
}