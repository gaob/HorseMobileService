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
        /// <summary>
        /// Get the User basic info after log in.
        /// </summary>
        /// <returns></returns>
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

                    // Get the profile picture url.
                    dynamic picture = client.Get("me/picture?redirect=false&height=200&width=200");

                    //Check to add User
                    string me_id = me.id;
                    var result = context.UserItems.Find(me_id);
                    bool isAdmin = false;

                    // If it's a first time user, log the person in the UserItems table.
                    if (result == null)
                    {
                        context.UserItems.Add(new UserItem { Id = me_id, Name = me.name, Pic_url = picture.data.url, isAdmin = false});
                    }
                    else
                    {
                        isAdmin = result.isAdmin;
                    }

                    //Check User's default horse
                    string horse_id = string.Empty;
                    string horse_name = string.Empty;
                    var query_horse = context.HorseItems.Where(horse => (horse.Owner_id == me_id));

                    if (query_horse.Count() > 0)
                    {
                        horse_id = query_horse.First().Id;
                        horse_name = query_horse.First().Name;
                    }

                    //Make it an Async method to save response time.
                    await context.SaveChangesAsync();

                    return Request.CreateResponse(HttpStatusCode.OK, new { id = me_id, 
                                                                           name = me.name,
                                                                           pic_url = picture.data.url,
                                                                           horse_id = horse_id,
                                                                           horse_name = horse_name,
                                                                           isAdmin = isAdmin});
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
