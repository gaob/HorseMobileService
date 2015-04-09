﻿using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using HorseMobileService.DataObjects;
using HorseMobileService.Models;

namespace HorseMobileService.Controllers
{
    public class HorseItemController : TableController<HorseItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<HorseItem>(context, Request, Services);
        }

        // GET tables/HorseItem
        public IQueryable<HorseItem> GetAllHorseItem()
        {
            return Query(); 
        }

        // GET tables/HorseItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<HorseItem> GetHorseItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/HorseItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<HorseItem> PatchHorseItem(string id, Delta<HorseItem> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/HorseItem
        public async Task<IHttpActionResult> PostHorseItem(HorseItem item)
        {
            HorseItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/HorseItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteHorseItem(string id)
        {
             return DeleteAsync(id);
        }

    }
}