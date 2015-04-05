using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using HorseMobileService.DataObjects;
using HorseMobileService.Models;

namespace HorseMobileService.Controllers
{
    public class UserItemController : TableController<UserItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<UserItem>(context, Request, Services);
        }

        // GET tables/UserItem
        public IQueryable<UserItem> GetAllUserItem()
        {
            return Query(); 
        }

        // GET tables/UserItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<UserItem> GetUserItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/UserItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<UserItem> PatchUserItem(string id, Delta<UserItem> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/UserItem
        public async Task<IHttpActionResult> PostUserItem(UserItem item)
        {
            UserItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/UserItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteUserItem(string id)
        {
             return DeleteAsync(id);
        }

    }
}