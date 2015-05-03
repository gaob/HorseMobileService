using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Notifications;

namespace HorseMobileService.Controllers
{
    public class notificationController : ApiController
    {
        public ApiServices Services { get; set; }

        /// <summary>
        /// Posts the specified notification message using a template
        /// </summary>
        /// <param name="notificationMessage">The notification message.</param>
        /// <param name="category">The category.</param>
        /// <returns>System.String.</returns>
        public async Task<HttpResponseMessage> Post(string notificationMessage, string category)
        {
            string googleMessage = @"{ ""data"" : {""msg"":"""+ notificationMessage+ @"""}}";

            string appleMessage = @"{ ""aps"" : {""alert"":""" + notificationMessage + @"""}}";

            try
            {

                NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString("Endpoint=sb://dotnet3hub-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=vVp4C3reoryHpsR1TlN5l6qbnVX+cg7L7vsmIF4CpN0=", "dotnet3hub");

                NotificationOutcome result;

                /*
                if (category == "GoogleApp" || true)
                {
                */
                    result = await hub.SendGcmNativeNotificationAsync(googleMessage, new string[] {category});
                    Services.Log.Info(result.State.ToString());
                    Services.Log.Info(string.Format("Category {0}, Sent Message: {1}", category, googleMessage));
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = googleMessage, pushResultState = result.State.ToString() });
                /*
                }
                */

                /*
                if (category == "AppleApp")
                {
                    result = await hub.SendAppleNativeNotificationAsync(appleMessage, new string[] { category });
                    Services.Log.Info(result.State.ToString());
                    Services.Log.Info(string.Format("Category {0}, Sent Message: {1}", category, appleMessage));
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = appleMessage, pushResultState = result.State.ToString() });
                }
                */

                //return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = "Category not supported" });
            }
            catch (Exception ex)
            {
                Services.Log.Error(ex.Message, null, "Push.SendAsync Error");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message });
            }
        }
    }
}
