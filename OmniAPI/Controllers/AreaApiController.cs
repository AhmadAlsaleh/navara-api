using Microsoft.AspNetCore.Mvc;
using SmartLifeLtd.Data.DataContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmniWeb.Controllers.Mobile
{
    [Route("api/[controller]/[Action]")]
    public class AreaApiController : Controller
    {
        private readonly OmniDbContext _context;
        private readonly LogDbContext _logContext;
        public AreaApiController(OmniDbContext context, LogDbContext LogDbContext)
        {
            _context = context;
            _logContext = LogDbContext;
        
        }
        /*Tarek [HttpGet]

        public IActionResult GetCountries()
        {
            try
            {
                var Countries = _context.Countries?.Select(x =>
                new
                {
                    ID = x.ID,
                    Name = x.Name,
                    Type = "Country",
                    ParentID = ""
                }).ToList();
                if (Countries?.Count > 0)
                {
                    return Ok(Countries);
                }
                else
                {
                    return Ok("[]");
                }
            }
            catch (Exception)
            {

                return BadRequest();
            }
            
        }
        [HttpGet]
        public IActionResult GetCities()
        {
            try
            {
                var Cities = _context.Cities?.Include("Country")?.Select(x =>
                new
                {
                    ID = x.ID,
                    Name = x.Name,
                    Type = "City",
                    ParentID = x.CountryID
                }).ToList();
                if (Cities?.Count > 0)
                {
                    return Ok(Cities);
                }
                else
                {
                    return Ok("[]");
                }
            }
            catch (Exception)
            {

                return BadRequest();
            }

        }
        [HttpGet]
        public IActionResult GetAreas()
        {
            try
            {
                var Areas = _context.Areas?.Include("City")?.Select(x =>
                new
                {
                    ID = x.ID,
                    Name = x.Name,
                    Type = "Area",
                    ParentID = x.CityID
                }).ToList();
                if (Areas?.Count > 0)
                {
                    return Ok(Areas);
                }
                else
                {
                    return Ok("[]");
                }
            }
            catch (Exception)
            {

                return BadRequest();
            }

        }
   */ }
}
