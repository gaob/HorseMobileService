using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HorseMobileService.Models;
using HorseMobileService.DataObjects;

namespace CustomAPIAMobileService.Controllers
{
    public class meController : ApiController
    {
        public ApiServices Services { get; set; }
        MobileServiceContext context = new MobileServiceContext();

        // GET api/me
        [AuthorizeLevel(AuthorizationLevel.User)]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                ServiceUser theUser = User as ServiceUser;

                Collection<ProviderCredentials> providerCredentialCollection = await theUser.GetIdentitiesAsync();

                if (providerCredentialCollection != null && providerCredentialCollection.Count > 0)
                {
                    FacebookCredentials theCredentials = providerCredentialCollection.OfType<FacebookCredentials>().FirstOrDefault();

                    Facebook.FacebookClient client = new Facebook.FacebookClient(theCredentials.AccessToken);

                    dynamic me = client.Get("me");

                    dynamic picture = client.Get("me/picture?redirect=false&height=200&width=200");

                    //Check to add User
                    var result = context.UserItems.Find(me.id);

                    if (result == null)
                    {
                        context.UserItems.Add(new UserItem { Id = me.id, Name = me.name, Pic_url = picture.data.url });
                    }
                    context.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, new { id = me.id, 
                                                                           name = me.name,
                                                                           pic_url = picture.data.url});
                }
                else
                {
                    throw new Exception("No Identities!");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
    }
}
