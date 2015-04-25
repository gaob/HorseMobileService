using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace HorseMobileService.Controllers
{
    public class queueController : ApiController
    {
        public ApiServices Services { get; set; }

        // GET api/queue
        [AuthorizeLevel(AuthorizationLevel.User)]
        public HttpResponseMessage Post([FromBody]dynamic payload)
        {
            try
            {
                if (payload.filename == null)
                {
                    throw new Exception("key not found!");
                }

                string filename = payload.filename;

                AddQueueMessage(filename);

                return Request.CreateResponse(HttpStatusCode.OK, new { filename = filename });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Adds the queue message.
        /// </summary>
        /// <param name="fileName">Name of the BLOB.</param>
        private void AddQueueMessage(string fileName)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=dotnet3;AccountKey=zyr2j7kSfhuf3BxySWXTMrpzlNUO4YFl6+kOIaD4uHJKK1jWV9aQr4gzx7eVJ33auScvc49vRhtQcgIMjlq0rA==");

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference("imagesqueue");

            // Create the queue if it doesn't already exist.
            bool isNew = queue.CreateIfNotExists();

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(fileName);
            queue.AddMessage(message);
        }
    }
}
