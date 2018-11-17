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
using SmartLifeLtd.Data.Tables.Shared;
using SmartLifeLtd.Enums;
using SmartLifeLtd.Management.Interfaces;

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
                var offer = await _context.Set<Offer>()
                    .Include(x => x.Item)
                    .Include(x => x.OfferItems)
                        .ThenInclude(x => x.Item)
                            .ThenInclude(x => x.ItemCategory)
                    .Include(x => x.OfferImages)
                    .FirstOrDefaultAsync(x => x.ID == id);
                if (offer == null) return NotFound("id is not related to any Offer");
                var json = new JsonResult(new OfferFullModel()
                {
                    ID = offer.ID,
                    Title = offer.Title,
                    ItemName = offer.Item?.Name,
                    Price = offer.Price ?? ((offer.Item?.Price ?? 0) - ((offer.Item?.Price ?? 0) * (offer.Discount ?? 0) / 100.0)),
                    Discount = offer.Discount,
                    UnitNetPrice = offer.Price ?? ((offer.Item?.Price ?? 0) - ((offer.Item?.Price ?? 0) * (offer.Discount ?? 0) / 100.0)),
                    UnitPrice = offer.Item?.Price ?? 0,
                    Description = offer.Description,
                    ThumbnailImagePath = offer.Item?.ThumbnailImagePath,
                    IsActive = offer.IsActive,
                    ShortDescription = offer.ShortDescription,
                    OfferType = offer.OfferType,
                    ItemID = offer.ItemID,
                    OfferImages = offer.OfferImages?.Select(y => y.ImagePath).ToList(),
                    OfferItems = offer.OfferItems?.Select(y => new ItemBasicModel()
                    {
                        ID = y.ItemID ?? Guid.Empty,
                        Name = y.Item?.Name,
                        Price = y.Item?.Price,
                        Quantity = y.Item?.Quantity,
                        ShortDescription = y.Item?.ShortDescription,
                        ItemCategoryID = y.Item?.ItemCategoryID,
                        ThumbnailImagePath = y.Item?.ThumbnailImagePath,
                        ItemCategory = y.Item?.ItemCategory?.Name
                    }).ToList()
                });
                #region Add Click History
                var clickContext = _context as IClickHistoryContext;
                if (clickContext != null)
                {
                    var clickObject = clickContext.ObjectClicks.FirstOrDefault(x => x.ObjectType == nameof(offer) && x.ObjectID == id);
                    if (clickObject == null)
                    {
                        clickObject = new SmartLifeLtd.Data.Tables.Shared.ObjectClick()
                        {
                            ObjectType = nameof(Offer),
                            ObjectID = id,
                            ClicksCount = 0
                        };
                        clickContext.ObjectClicks.Add(clickObject);
                    }
                    clickObject.ClicksCount++;
                    clickObject.LastClickedDate = DateTime.UtcNow;
                    _context.SaveChanges();
                }
                #endregion

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
                var data = _context.Set<Offer>().Include(x => x.Item).ToList();
                var json = new JsonResult(data.Select(x => new OfferBasicModel()
                {
                    ID = x.ID,
                    Title = x.Title,
                    ItemName = x.Item?.Name,
                    ThumbnailImagePath = x.ThumbnailImagePath,
                    ShortDescription = x.ShortDescription,
                    OfferType = x.OfferType,
                    Discount = x.Discount,
                    UnitNetPrice = x.Price ?? ((x.Item?.Price ?? 0) - ((x.Item?.Price ?? 0) * (x.Discount ?? 0) / 100.0)),
                    UnitPrice = x.Item?.Price
                }));
                #region Add Open View History
                var clickContext = _context as IClickHistoryContext;
                if (clickContext != null)
                {
                    var clickView = clickContext.OpenViewHistories.FirstOrDefault(x =>
                        x.View == NavaraView.Offer.ToString() &&
                        x.Date.GetValueOrDefault().Date == DateTime.Now.Date);
                    if (clickView == null)
                    {
                        clickView = new OpenViewHistory()
                        {
                            ClickTime = 0,
                            View = NavaraView.Offer.ToString(),
                            Date = DateTime.Now.Date
                        };
                        clickContext.OpenViewHistories.Add(clickView);
                    }
                    clickView.ClickTime++;
                    clickContext.SubmitAsync();
                }
                #endregion
                return json;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
