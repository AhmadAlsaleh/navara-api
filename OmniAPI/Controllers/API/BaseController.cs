using SmartLifeLtd.Data.Tables.Omni;using SmartLifeLtd.Data.AspUsers;using SmartLifeLtd.Data.DataContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using SmartLifeLtd.Data.Tables;
using Microsoft.AspNetCore.Identity;
using SmartLifeLtd.Classes;

namespace OmniAPI.Controllers
{
    [Route("api/[controller]")]
    public class BaseController<T> : Controller
        where T : TableBase
    {
        protected readonly OmniDbContext _context;
        protected readonly LogDbContext _logContext;
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseController(OmniDbContext context, LogDbContext logContext, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logContext = logContext;
            _userManager = userManager;
        }

        [HttpGet]
        public virtual async Task<IActionResult> Get()
        {
            try
            {
                var data = _context.Set<T>().ToList();
                var json = new JsonResult(data);
                return json;
            }
            catch(Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        [ActionName("Get")]
        public virtual async Task<IActionResult> GetById(Guid id)
        {
            var item = _context.Set<T>().SingleOrDefault(x => x.ID == id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] T item)
        {
            if (item == null)
            {
                return BadRequest();
            }
            try
            {
                _context.Set<T>().Add(item);
                _context.SubmitAsync();
                return CreatedAtRoute("Get", new { id = item.ID }, item);
            }
            catch (Exception)
            {

                return BadRequest();
            }

        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update(Guid ID, [FromBody] T item)
        {
            if (item == null || item.ID != ID)
            {
                return BadRequest();
            }

            var orginal = _context.Set<T>().AsNoTracking().SingleOrDefault(x => x.ID == ID);
            if (orginal == null)
            {
                return NotFound();
            }

            _context.Entry<T>(item).State = EntityState.Modified;
            await _context.SubmitAsync();
            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(Guid ID)
        {
            var item = _context.Set<T>().SingleOrDefault(x => x.ID == ID);
            if (item == null)
            {
                return NotFound();
            }

            _context.Set<T>().Remove(item);
            await _context.SubmitAsync();
            return new NoContentResult();
        }
    }
}
