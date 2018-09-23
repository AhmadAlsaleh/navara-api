using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Sync;
using SmartLifeLtd.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NavaraAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class OperationsController : Controller
    {    
        private NavaraDbContext _Context { set; get; }
        private IHostingEnvironment env { set; get; }

        #region Constructer
        /// <summary>
        /// Default constructer
        /// </summary>
        public OperationsController(NavaraDbContext context, IHostingEnvironment env)
        {
            _Context = context;
            this.env = env;
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> DownloadFile([FromBody]FileViewModel item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.FilePath)) return BadRequest();
            var path = Path.Combine(this.env.WebRootPath, item.FilePath);
            if (!System.IO.File.Exists(path)) return NotFound("File is not exist");
            try
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, MMITypes.GetContentType(path), Path.GetFileName(path));
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ContactUs([FromBody]ContactUsViewModel model)
        {
            try
            {
                //EmailService.SendEmail(model.Name, model.Email, model.Message, "Contact us");
                return this.Content("Message has been sent successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
