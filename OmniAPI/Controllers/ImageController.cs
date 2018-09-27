using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OmniAPI.Classes;
using OmniAPI.ViewModel;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Omni.Controllers.API
{
    [Route("api/[controller]/[Action]")]
    public class ImageController : Controller
    {
        private readonly OmniDbContext _context;
        private readonly LogDbContext _logContext;

        public ImageController(OmniDbContext context, LogDbContext LogDbContext)
        {
            _context = context;
            _logContext = LogDbContext;
        }

        public static List<string> GetNewImage(LogDbContext _logContext, string _clientKey, DateRange dateRange)
        {
            var IDs = _logContext.LogDbOperations.Where(x =>
                        x.Client != _clientKey &&
                        x.OperationType == OperationType.AddFile.ToString() /*&&
                        x.CreationDate >= dateRange.FromDate &&
                        x.CreationDate < dateRange.ToDate*/).Select(x => x.Remark).ToList();
            foreach (var id in IDs.ToList())
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", id);
                if (!System.IO.File.Exists(fullPath))
                    IDs.Remove(id);
            }
            return IDs;
        }

        public static List<string> GetDeletedImage(LogDbContext _logContext, string _clientKey, DateRange dateRange)
        {
            var images = _logContext.LogDbOperations.Where(x =>
                    x.Client != _clientKey &&
                    x.OperationType == OperationType.DeleteFile.ToString() /*&&
                    x.CreationDate >= dateRange.FromDate &&
                    x.CreationDate < dateRange.ToDate*/).Select(x => x.Remark).ToList();
            return images;
        }

        [HttpPost]
        public virtual async Task<IActionResult> PullNewFiles([FromBody] DateRange dateRange)
        {
            if (dateRange == null) return BadRequest();
            try
            {
                string clientKey = this.Request.Headers["ClientKey"];
                List<string> data = GetNewImage(_logContext, clientKey, dateRange);
                var json = new JsonResult(data);
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> PullDeletedFiles([FromBody] DateRange dateRange)
        {
            if (dateRange == null) return BadRequest();
            try
            {
                string clientKey = this.Request.Headers["ClientKey"];
                List<Guid> data = new List<Guid>();
                var images = GetDeletedImage(_logContext, clientKey, dateRange);
                var json = new JsonResult(images);
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FileName);
            var dir = Path.GetDirectoryName(path);
            try
            {
                string clientKey = this.Request.Headers["ClientKey"];
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (Exception ex) { return Json(ex.Message); }
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    _logContext.LogDbOperations.Add(new SmartLifeLtd.Data.Tables.Log.LogDbOperation()
                    {
                        Client = clientKey,
                        CreationDate = DateTime.Now,
                        OperationType = OperationType.AddFile.ToString(),
                        Remark = file.FileName
                    });
                    _logContext.SubmitAsync();
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFiles([FromBody] List<string> files)
        {
            if (files == null) return BadRequest();
            try
            {
                foreach (string file in files)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file);
                    var dir = Path.GetDirectoryName(file);
                    if (System.IO.File.Exists(file))
                    {
                        string clientKey = this.Request.Headers["ClientKey"];
                        System.IO.File.Delete(file);
                        _logContext.LogDbOperations.Add(new SmartLifeLtd.Data.Tables.Log.LogDbOperation()
                        {
                            Client = clientKey,
                            CreationDate = DateTime.Now,
                            OperationType = OperationType.AddFile.ToString(),
                            Remark = file
                        });
                        _logContext.SubmitAsync();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DownloadImage([FromBody]ItemImg item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.ImagePath)) return BadRequest();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", item.ImagePath);
            if (!System.IO.File.Exists(path)) return BadRequest();
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
        public async Task<IActionResult> DownloadFile([FromBody]FileViewModel item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.FilePath)) return BadRequest();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", item.FilePath);
            if (!System.IO.File.Exists(path)) return BadRequest();
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

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetAdMainImage(Guid ID)
        {
            var item = _context.ADs.Include("AdImages").SingleOrDefault(x => x.ID == ID);
            if (item == null) return null;
            string filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\ItemImages\\{item.Code}\\{item.ADImages.FirstOrDefault(x => x.IsMain == true).ImagePath}";
            var fs = new FileStream(filePath, FileMode.Open);
            string fileType = MMIWebTypes.GetContentType(Path.GetExtension(filePath));
            return File(fs, fileType);
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetAdMainThumbnail(Guid ID)
        {
            var item = _context.ADs.Include("AdImages").SingleOrDefault(x => x.ID == ID);
            if (item == null) return null;
            string filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\ItemImages\\{item.Code}\\{item.ADImages.FirstOrDefault(x => x.IsMain == true).SubNelImagePath}";
            if(!System.IO.File.Exists(filePath))
                filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\No-image-found.jpg";
            var fs = new FileStream(filePath, FileMode.Open);
            string fileType = MMIWebTypes.GetContentType(Path.GetExtension(filePath));
            return File(fs, fileType);
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetAdImage(Guid ID)
        {
            var item = _context.ADImages.Include("Ad").SingleOrDefault(x => x.ID == ID);
            if (item == null) return null;
            string filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\ItemImages\\{item.AD.Code}\\{item.ImagePath}";
            if (!System.IO.File.Exists(filePath))
                filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\No-image-found.jpg";
            var fs = new FileStream(filePath, FileMode.Open);
            string fileType = MMIWebTypes.GetContentType(Path.GetExtension(filePath));
            return File(fs, fileType);
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetAdThumbnail(Guid ID)
        {
            var item = _context.ADImages.Include("Ad").SingleOrDefault(x => x.ID == ID);
            if (item == null) return null;
            string filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\ItemImages\\{item.AD.Code}\\{item.SubNelImagePath}";
            if (!System.IO.File.Exists(filePath))
                filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\No-image-found.jpg";
            var fs = new FileStream(filePath, FileMode.Open);
            string fileType = MMIWebTypes.GetContentType(Path.GetExtension(filePath));
            return File(fs, fileType);
        }

        [HttpGet("{ID}")]
        public async Task<IActionResult> GetCategoryImage(Guid ID)
        {
            var item = _context.Categories.SingleOrDefault(x => x.ID == ID);
            if (item == null) return null;
            string filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\Categories\\{item.ImagePath}";
            if (!System.IO.File.Exists(filePath))
                filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\images\\No-image-found.jpg";
            var fs = new FileStream(filePath, FileMode.Open);
            string fileType = MMIWebTypes.GetContentType(Path.GetExtension(filePath));
            return File(fs, fileType);
        }

    }

    public class ItemImg
    {
        public string ImagePath { get; set; }
    }

    public class MMITypes
    {
        static Dictionary<String, String> mimeTypes = new Dictionary<String, String>
            {
                {".bmp", "image/bmp"},
                {".gif", "image/gif"},
                {".jpeg", "image/jpeg"},
                {".jpg", "image/jpeg"},
                {".png", "image/png"},
                {".tif", "image/tiff"},
                {".tiff", "image/tiff"},
                {".doc", "application/msword"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".pdf", "application/pdf"},
                {".ppt", "application/vnd.ms-powerpoint"},
                {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".xls", "application/vnd.ms-excel"},
                {".csv", "text/csv"},
                {".xml", "text/xml"},
                {".txt", "text/plain"},
                {".zip", "application/zip"},
                {".ogg", "application/ogg"},
                {".mp3", "audio/mpeg"},
                {".wma", "audio/x-ms-wma"},
                {".wav", "audio/x-wav"},
                {".wmv", "audio/x-ms-wmv"},
                {".swf", "application/x-shockwave-flash"},
                {".avi", "video/avi"},
                {".mp4", "video/mp4"},
                {".mpeg", "video/mpeg"},
                {".mpg", "video/mpeg"},
                {".qt", "video/quicktime"}
            };


        public static string GetContentType(string fileExtension)
        {
            // if the file type is not recognized, return "application/octet-stream" so the browser will simply download it
            return mimeTypes.ContainsKey(fileExtension) ? mimeTypes[fileExtension] : "application/octet-stream";
        }
    }
}
