using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.ViewModels;
using SmartLifeLtd.API;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables.Navara;

namespace NavaraAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class OffersController : BaseController<Offer>
    {
        public OffersController(NavaraDbContext context)
            : base(context)
        {

        }

        [HttpGet("{id}")]
        [ActionName("Get")]
        public override async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var offer = await _context.Set<Offer>().FirstOrDefaultAsync(x => x.ID == id);
                if (offer == null) return NotFound("id is not related to any Offer");
                var json = new JsonResult(new OfferFullModel()
                {
                    ID = offer.ID,
                    Title = offer.Title,
                    Price = offer.Price,
                    Discount = offer.Discount,
                    Description = offer.Description,
                    ThumbnailImagePath = offer.ThumbnailImagePath,
                    IsActive = offer.IsActive,
                    ShortDescription = offer.ShortDescription,
                    OfferType = offer.OfferType,
                    ItemID = offer.ItemID,
                    OfferImages = offer.OfferImages?.Select(y => y.ImagePath).ToList(),
                    OfferItems = offer.OfferItems?.Select(y => y.ItemID.GetValueOrDefault()).ToList()
                });
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBasic()
        {
            try
            {
                var data = _context.Set<Offer>().ToList();
                var json = new JsonResult(data.Select(x => new OfferBasicModel()
                {
                    ID = x.ID,
                    Title = x.Title,
                    ThumbnailImagePath = x.ThumbnailImagePath,
                    ShortDescription = x.ShortDescription,
                    OfferType = x.OfferType,
                    Discount = x.Discount
                }));
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
