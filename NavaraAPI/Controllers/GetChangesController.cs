using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NavaraAPI.ViewModels;
using SmartLifeLtd.API;
using SmartLifeLtd.Classes.Attribute;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Data.DataContexts;
using SmartLifeLtd.Data.Tables;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Enums;
using SmartLifeLtd.Models;

namespace NavaraAPI.Controllers
{
    [Route("[controller]/[action]")]
    public class GetChangesController : Controller
    {
        private NavaraDbContext _Context { set; get; }
        public GetChangesController(NavaraDbContext Context)
        {
            _Context = Context;
        }

        private async Task<IActionResult> GetChanges<table>(GetChangesModel model) where table : DatabaseObject
        {
            if (model == null) return null;
            var Inserted = await _Context.Set<table>().Where(x => x.CreationDate >= model.LastUpdate).ToListAsync();
            var Updated = await _Context.Set<table>().Where(x => x.UpdatedDate >= model.LastUpdate && x.CreationDate <= model.LastUpdate).ToListAsync();
            var IDs = await _Context.Set<table>().Select(x => x.ID).ToListAsync();
            var Deleted = model.RecoredIDs.Where(x => !IDs.Contains(x)).ToList();
            return Json(new
            {
                Inserted,
                Updated,
                Deleted
            });
        }

        [HttpPost]
        public async Task<IActionResult> Offer([FromBody]GetChangesModel model) {
            if (model == null) return null;
            var Inserted = await _Context.Set<Offer>().Where(x => x.CreationDate >= model.LastUpdate).Select(x =>
                new OfferModel
                {
                    Description = x.Description,
                    Discount = x.Discount,
                    IsActive = x.IsActive,
                    OfferType = x.OfferType,
                    ItemID = x.ItemID,
                    Price = x.Price,
                    ShortDescription = x.Description,
                    ThumbnailImagePath = x.ThumbnailImagePath,
                    Title = x.Title
                }).ToListAsync();
            var Updated = await _Context.Set<Offer>().Where(x => x.UpdatedDate >= model.LastUpdate && x.CreationDate <= model.LastUpdate).Select(x =>
                new OfferModel
                {
                    Description = x.Description,
                    Discount = x.Discount,
                    IsActive = x.IsActive,
                    OfferType = x.OfferType,
                    ItemID = x.ItemID,
                    Price = x.Price,
                    ShortDescription = x.Description,
                    ThumbnailImagePath = x.ThumbnailImagePath,
                    Title = x.Title
                }).ToListAsync();
            var IDs = await _Context.Set<Offer>().Select(x => x.ID).ToListAsync();
            var Deleted = model.RecoredIDs.Where(x => !IDs.Contains(x)).ToList();
            return Json(new
            {
                Inserted,
                Updated,
                Deleted
            });
        }

        [HttpPost]
        public async Task<IActionResult> OfferImage([FromBody]GetChangesModel model) {
            if (model == null) return null;
            var Inserted = await _Context.Set<OfferImage>().Where(x => x.CreationDate >= model.LastUpdate).Select(x =>
                new OfferImageModel
                {
                    ImagePath = x.ImagePath,
                    IsMain = x.IsMain,
                    OfferID = x.OfferID
                }).ToListAsync();
            var Updated = await _Context.Set<OfferImage>().Where(x => x.UpdatedDate >= model.LastUpdate && x.CreationDate <= model.LastUpdate).Select(x =>
                new OfferImageModel
                {
                    ImagePath = x.ImagePath,
                    IsMain = x.IsMain,
                    OfferID = x.OfferID
                }).ToListAsync();
            var IDs = await _Context.Set<OfferImage>().Select(x => x.ID).ToListAsync();
            var Deleted = model.RecoredIDs.Where(x => !IDs.Contains(x)).ToList();
            return Json(new
            {
                Inserted,
                Updated,
                Deleted
            });
        }

        [HttpPost]
        public async Task<IActionResult> OfferItem([FromBody]GetChangesModel model) {
            if (model == null) return null;
            var Inserted = await _Context.Set<OfferItem>().Where(x => x.CreationDate >= model.LastUpdate).Select(x =>
                new OfferItemModel
                {
                    ItemID = x.ItemID,
                    Quantity = x.Quantity,
                    OfferID = x.OfferID
                }).ToListAsync();
            var Updated = await _Context.Set<OfferItem>().Where(x => x.UpdatedDate >= model.LastUpdate && x.CreationDate <= model.LastUpdate).Select(x =>
                new OfferItemModel
                {
                    ItemID = x.ItemID,
                    Quantity = x.Quantity,
                    OfferID = x.OfferID
                }).ToListAsync();
            var IDs = await _Context.Set<OfferItem>().Select(x => x.ID).ToListAsync();
            var Deleted = model.RecoredIDs.Where(x => !IDs.Contains(x)).ToList();
            return Json(new
            {
                Inserted,
                Updated,
                Deleted
            });
        }

        [HttpPost]
        public async Task<IActionResult> Item([FromBody]GetChangesModel model) {
            if (model == null) return null;
            var Inserted = await _Context.Set<Item>().Where(x => x.CreationDate >= model.LastUpdate).Select(x =>
                new ItemModel
                {
                    Description = x.Description,
                    Quantity = x.Quantity,
                    IsEnable = x.IsEnable,
                    ItemCategoryID = x.ItemCategoryID,
                    Name = x.Name,
                    Price = x.Price,
                    ShortDescription = x.ShortDescription,
                    ThumbnailImagePath = x.ThumbnailImagePath
                }).ToListAsync();
            var Updated = await _Context.Set<Item>().Where(x => x.UpdatedDate >= model.LastUpdate && x.CreationDate <= model.LastUpdate).Select(x =>
                new ItemModel
                {
                    Description = x.Description,
                    Quantity = x.Quantity,
                    IsEnable = x.IsEnable,
                    ItemCategoryID = x.ItemCategoryID,
                    Name = x.Name,
                    Price = x.Price,
                    ShortDescription = x.ShortDescription,
                    ThumbnailImagePath = x.ThumbnailImagePath
                }).ToListAsync();
            var IDs = await _Context.Set<Item>().Select(x => x.ID).ToListAsync();
            var Deleted = model.RecoredIDs.Where(x => !IDs.Contains(x)).ToList();
            return Json(new
            {
                Inserted,
                Updated,
                Deleted
            });
        }

        [HttpPost]
        public async Task<IActionResult> ItemImage([FromBody]GetChangesModel model) {
            if (model == null) return null;
            var Inserted = await _Context.Set<ItemImage>().Where(x => x.CreationDate >= model.LastUpdate).Select(x =>
                new ItemImageModel
                {
                    ItemID = x.ItemID,
                    ImagePath = x.ImagePath,
                    IsMain = x.IsMain
                }).ToListAsync();
            var Updated = await _Context.Set<ItemImage>().Where(x => x.UpdatedDate >= model.LastUpdate && x.CreationDate <= model.LastUpdate).Select(x =>
                new ItemImageModel
                {
                    ItemID = x.ItemID,
                    ImagePath = x.ImagePath,
                    IsMain = x.IsMain
                }).ToListAsync();
            var IDs = await _Context.Set<ItemImage>().Select(x => x.ID).ToListAsync();
            var Deleted = model.RecoredIDs.Where(x => !IDs.Contains(x)).ToList();
            return Json(new
            {
                Inserted,
                Updated,
                Deleted
            });
        }

        [HttpPost]
        public async Task<IActionResult> ItemCategory([FromBody]GetChangesModel model) {
            if (model == null) return null;
            var Inserted = await _Context.Set<ItemCategory>().Where(x => x.CreationDate >= model.LastUpdate).Select(x =>
                new ItemCategoryModel
                {
                    Description = x.Description,
                    ImagePath = x.ImagePath,
                    Name = x.Name
                }).ToListAsync();
            var Updated = await _Context.Set<ItemCategory>().Where(x => x.UpdatedDate >= model.LastUpdate && x.CreationDate <= model.LastUpdate).Select(x =>
                new ItemCategoryModel
                {
                    Description = x.Description,
                    ImagePath = x.ImagePath,
                    Name = x.Name
                }).ToListAsync();
            var IDs = await _Context.Set<ItemCategory>().Select(x => x.ID).ToListAsync();
            var Deleted = model.RecoredIDs.Where(x => !IDs.Contains(x)).ToList();
            return Json(new
            {
                Inserted,
                Updated,
                Deleted
            });
        }
    }

    public class OfferImageModel
    {
        public Guid ID { set; get; }
        public string ImagePath { get; set; }
        public Guid? OfferID { get; set; }
        public bool? IsMain { get; set; }
    }
    public class OfferItemModel
    {
        public Guid ID { set; get; }
        public int? Quantity { get; set; }
        public Guid? OfferID { get; set; }
        public Guid? ItemID { get; set; }
    }
    public class ItemCategoryModel
    {
        public Guid ID { set; get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { set; get; }
    }
    public class ItemImageModel
    {
        public Guid ID { set; get; }
        public string ImagePath { get; set; }
        public Guid? ItemID { get; set; }
        public bool? IsMain { get; set; }
    }
}
