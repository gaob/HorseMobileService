using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using HorseMobileService.DataObjects;

namespace HorseMobileService.Controllers
{
    public class tableController : ApiController
    {
        public ApiServices Services { get; set; }

        // GET api/queue
        [AuthorizeLevel(AuthorizationLevel.User)]
        public HttpResponseMessage Post([FromBody]dynamic payload)
        {
            try
            {
                if (payload.author_id == null || 
                    payload.author_name == null ||
                    payload.author_pic_url == null ||
                    payload.text == null ||
                    payload.pic_url == null ||
                    payload.publishtime == null ||
                    payload.horse_id == null)
                {
                    throw new Exception("key not found!");
                }

                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=dotnet3;AccountKey=zyr2j7kSfhuf3BxySWXTMrpzlNUO4YFl6+kOIaD4uHJKK1jWV9aQr4gzx7eVJ33auScvc49vRhtQcgIMjlq0rA==");

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Create the CloudTable object that represents the "logentry" table.
                CloudTable table = tableClient.GetTableReference("newstable");

                // Create the table if it does not exist
                bool isNew = table.CreateIfNotExists();

                NewsItem anItem = new NewsItem();

                // Assign a unique row key
                anItem.RowKey = Guid.NewGuid().ToString();

                anItem.Author_id = payload.author_id;
                anItem.Author_name = payload.author_name;
                anItem.Author_pic_url = payload.author_pic_url;
                anItem.Text = payload.text;
                anItem.Pic_url = payload.pic_url;
                anItem.PublishTime = DateTime.Parse((string)payload.publishtime);
                anItem.Horse_id = payload.horse_id;

                anItem.PartitionKey = anItem.PublishTime.ToString("MMddyyyy");

                // Create the TableOperation that inserts the external log entry entity.
                TableOperation insertOperation = TableOperation.Insert(anItem);

                // Execute the insert operation.
                TableResult tableResult = table.Execute(insertOperation);

                if (tableResult.HttpStatusCode == 204)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { rowkey = anItem.RowKey });
                }
                else
                {
                    return Request.CreateResponse((HttpStatusCode)tableResult.HttpStatusCode, new { rowkey = anItem.RowKey });
                }
                
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
    }
}
