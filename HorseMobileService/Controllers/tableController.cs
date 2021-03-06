﻿using System;
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
using Newtonsoft.Json.Linq;
using Microsoft.ServiceBus.Notifications;

namespace HorseMobileService.Controllers
{
    public class tableController : ApiController
    {
        private const string NEWS_PARTITIONKEY = "NEWS";
        public ApiServices Services { get; set; }

        // GET api/table/news
        /// <summary>
        /// GET API to get all news.
        /// </summary>
        /// <returns></returns>
        [AuthorizeLevel(AuthorizationLevel.User)]
        [Route("api/table/news")]
        public HttpResponseMessage GetAllNews()
        {
            try
            {
                JArray JNews = new JArray();

                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=dotnet3;AccountKey=zyr2j7kSfhuf3BxySWXTMrpzlNUO4YFl6+kOIaD4uHJKK1jWV9aQr4gzx7eVJ33auScvc49vRhtQcgIMjlq0rA==");

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Create the CloudTable object that represents the "logentry" table.
                CloudTable table = tableClient.GetTableReference("newstable");

                // Create the table if it does not exist
                bool isNew = table.CreateIfNotExists();

                foreach (NewsItem item in table.ExecuteQuery(new TableQuery<NewsItem>()).Where(x => x.IsReady).OrderByDescending(x=>x.PublishTime))
                {
                    JNews.Add(JObject.FromObject(new
                    {
                        id = item.RowKey,
                        author_id = item.Author_id,
                        author_name = item.Author_name,
                        author_pic_url = item.Author_pic_url,
                        text = item.Text,
                        pic_url = item.Pic_url,
                        publishtime = item.PublishTime.ToString(),
                        horse_id = item.Horse_id,
                        horse_name = item.Horse_name,
                        like_count = item.Like_Count,
                        comment_count = item.Comment_Count
                    }));
                }

                return Request.CreateResponse(HttpStatusCode.OK, JNews);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        // GET api/table/news
        /// <summary>
        /// POST api to create news.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [AuthorizeLevel(AuthorizationLevel.User)]
        [Route("api/table/news")]
        public HttpResponseMessage PostNews([FromBody]dynamic payload)
        {
            try
            {
                if (payload.author_id == null || 
                    payload.author_name == null ||
                    payload.author_pic_url == null ||
                    payload.text == null ||
                    payload.pic_url == null ||
                    payload.publishtime == null ||
                    payload.horse_id == null ||
                    payload.horse_name == null)
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
                anItem.Pic_url = "https://dotnet3.blob.core.windows.net/dotnet3/news-" + anItem.RowKey + "-thumbnail.jpg";
                anItem.PublishTime = DateTime.Parse((string)payload.publishtime);
                anItem.Horse_id = payload.horse_id;
                anItem.Horse_name = payload.horse_name;

                anItem.PartitionKey = NEWS_PARTITIONKEY;

                // Internal field
                anItem.Like_Count = 0;
                anItem.Comment_Count = 0;
                anItem.IsReady = false;

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

        // GET api/table/news
        /// <summary>
        /// DELETE api to delete news.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthorizeLevel(AuthorizationLevel.User)]
        [Route("api/table/news/{id}")]
        public HttpResponseMessage DeleteNews(string id)
        {
            try
            {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=dotnet3;AccountKey=zyr2j7kSfhuf3BxySWXTMrpzlNUO4YFl6+kOIaD4uHJKK1jWV9aQr4gzx7eVJ33auScvc49vRhtQcgIMjlq0rA==");

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Create the CloudTable object that represents the "logentry" table.
                CloudTable table = tableClient.GetTableReference("newstable");

                var anOperation = TableOperation.Retrieve<NewsItem>(NEWS_PARTITIONKEY, id);

                var tableResult = table.Execute(anOperation);

                NewsItem theNews = (NewsItem)tableResult.Result;
                string author_id = theNews.Author_id;

                if (theNews == null)
                {
                    throw new Exception("Couldn't find theNews.");
                }

                var deleteOperation = TableOperation.Delete(theNews);

                table.Execute(deleteOperation);

                // Also delete all comments associated with this news.
                table = tableClient.GetTableReference("commentstable");

                var batchOperation = new TableBatchOperation();

                var projectionQuery = new TableQuery<CommentItem>()
                                        .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id))
                                        .Select(new string[] { "RowKey" });

                foreach (var e in table.ExecuteQuery(projectionQuery))
                {
                    batchOperation.Delete(e);
                }

                // If no comments, shouldn't run the batch operations, otherwise there will be exceptions.
                if (batchOperation.Count > 0)
                {
                    table.ExecuteBatch(batchOperation);
                }

                // Create a notification to inform the author that his/her post has been deleted.
                table = tableClient.GetTableReference("notificationstable");

                NotificationItem aNotification = new NotificationItem();

                // Assign a unique row key
                aNotification.RowKey = Guid.NewGuid().ToString();
                aNotification.PartitionKey = "NOTIFICATION";

                aNotification.User_id = author_id;
                aNotification.Text = "Your post from " + theNews.PublishTime.ToShortDateString() +" has been deleted by admin!";
                aNotification.Time = DateTime.Now;

                anOperation = TableOperation.Insert(aNotification);

                table.Execute(anOperation);

                NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString("Endpoint=sb://dotnet3hub-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=vVp4C3reoryHpsR1TlN5l6qbnVX+cg7L7vsmIF4CpN0=", "dotnet3hub");

                hub.SendGcmNativeNotificationAsync(@"{ ""data"" : {""msg"":""" + aNotification.Text + @"""}}", new string[] { author_id });

                return Request.CreateResponse(HttpStatusCode.OK, new { id = id});
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        // GET api/table/comment
        /// <summary>
        /// GET api to get all comments for news.
        /// </summary>
        /// <param name="news_id"></param>
        /// <returns></returns>
        [AuthorizeLevel(AuthorizationLevel.User)]
        [Route("api/table/comment/{news_id}")]
        public HttpResponseMessage GetComments(string news_id)
        {
            try
            {
                JArray JComments = new JArray();

                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=dotnet3;AccountKey=zyr2j7kSfhuf3BxySWXTMrpzlNUO4YFl6+kOIaD4uHJKK1jWV9aQr4gzx7eVJ33auScvc49vRhtQcgIMjlq0rA==");

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Create the CloudTable object that represents the "logentry" table.
                CloudTable table = tableClient.GetTableReference("commentstable");

                // Create the table if it does not exist
                bool isNew = table.CreateIfNotExists();

                foreach (CommentItem item in table.ExecuteQuery(new TableQuery<CommentItem>()).Where(x => x.News_id == news_id).OrderBy(x => x.PublishTime))
                {
                    JComments.Add(JObject.FromObject(new
                    {
                        id = item.RowKey,
                        author_id = item.Author_id,
                        author_name = item.Author_name,
                        text = item.Text,
                        publishtime = item.PublishTime.ToString(),
                        news_id = item.News_id,
                        liked = item.Liked ? "true" : "false"
                    }));
                }

                return Request.CreateResponse(HttpStatusCode.OK, JComments);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        // POST api/table/comment
        /// <summary>
        /// POST api to create a comment.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [AuthorizeLevel(AuthorizationLevel.User)]
        [Route("api/table/comment")]
        public HttpResponseMessage PostComment([FromBody]dynamic payload)
        {
            try
            {
                if (payload.author_id == null ||
                    payload.author_name == null ||
                    payload.text == null ||
                    payload.publishtime == null ||
                    payload.news_id == null ||
                    payload.liked == null)
                {
                    throw new Exception("key not found!");
                }

                string notification_message = string.Empty;

                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=dotnet3;AccountKey=zyr2j7kSfhuf3BxySWXTMrpzlNUO4YFl6+kOIaD4uHJKK1jWV9aQr4gzx7eVJ33auScvc49vRhtQcgIMjlq0rA==");

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Create the CloudTable object that represents the "logentry" table.
                CloudTable table = tableClient.GetTableReference("commentstable");

                // Create the table if it does not exist
                bool isNew = table.CreateIfNotExists();

                CommentItem anItem = new CommentItem();

                // Assign a unique row key
                anItem.RowKey = Guid.NewGuid().ToString();
                anItem.PartitionKey = (string)payload.news_id;

                anItem.Author_id = payload.author_id;
                anItem.Author_name = payload.author_name;
                anItem.Text = payload.text;
                anItem.PublishTime = DateTime.Parse((string)payload.publishtime);
                anItem.News_id = payload.news_id;
                anItem.Liked = bool.Parse((string)payload.liked);

                // Create the TableOperation that inserts the external log entry entity.
                TableOperation anOperation = TableOperation.Insert(anItem);

                // Execute the insert operation.
                TableResult tableResult = table.Execute(anOperation);

                // Create the CloudTable object that represents the "logentry" table.
                table = tableClient.GetTableReference("newstable");

                // Get the news to update like or comment count.
                anOperation = TableOperation.Retrieve<NewsItem>(NEWS_PARTITIONKEY, anItem.News_id);

                tableResult = table.Execute(anOperation);

                NewsItem theNews = (NewsItem)tableResult.Result;

                if (theNews != null)
                {
                    // Increment the like count for like action.
                    if (anItem.Liked)
                    {
                        theNews.Like_Count += 1;

                        notification_message = anItem.Author_name + " liked your post from " + theNews.PublishTime.ToShortDateString() + ".";
                    }
                    // Increment the comment count for comment action.
                    else
                    {
                        theNews.Comment_Count += 1;

                        notification_message = anItem.Author_name + " commented on your post from " + theNews.PublishTime.ToShortDateString() + ".";
                    }

                    anOperation = TableOperation.Replace(theNews);

                    table.Execute(anOperation);
                }

                string to_id = theNews.Author_id;

                // Create notification for the post author.
                table = tableClient.GetTableReference("notificationstable");

                NotificationItem aNotification = new NotificationItem();

                // Assign a unique row key
                aNotification.RowKey = Guid.NewGuid().ToString();
                aNotification.PartitionKey = "NOTIFICATION";

                aNotification.User_id = to_id;
                aNotification.Text = notification_message;
                aNotification.Time = DateTime.Now;

                anOperation = TableOperation.Insert(aNotification);

                table.Execute(anOperation);

                NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString("Endpoint=sb://dotnet3hub-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=vVp4C3reoryHpsR1TlN5l6qbnVX+cg7L7vsmIF4CpN0=", "dotnet3hub");

                // comment from post author himself/herself doesn't send notifications.
                if (anItem.Author_id != theNews.Author_id)
                {
                    hub.SendGcmNativeNotificationAsync(@"{ ""data"" : {""msg"":""" + notification_message + @"""}}", new string[] { to_id });
                }

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

        // GET api/table/notifications
        /// <summary>
        /// GET api to get all news.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthorizeLevel(AuthorizationLevel.User)]
        [Route("api/table/notifications/{id}")]
        public HttpResponseMessage GetAllNews(string id)
        {
            try
            {
                JArray JNofitications = new JArray();

                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=dotnet3;AccountKey=zyr2j7kSfhuf3BxySWXTMrpzlNUO4YFl6+kOIaD4uHJKK1jWV9aQr4gzx7eVJ33auScvc49vRhtQcgIMjlq0rA==");

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Create the CloudTable object that represents the "logentry" table.
                CloudTable table = tableClient.GetTableReference("notificationstable");

                // Create the table if it does not exist
                bool isNew = table.CreateIfNotExists();

                foreach (NotificationItem item in table.ExecuteQuery(new TableQuery<NotificationItem>()).Where(x => x.User_id == id).OrderByDescending(x => x.Time))
                {
                    JNofitications.Add(JObject.FromObject(new
                    {
                        id = item.RowKey,
                        user_id = item.User_id,
                        text = item.Text,
                        time = item.Time
                    }));
                }

                return Request.CreateResponse(HttpStatusCode.OK, JNofitications);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
    }
}
