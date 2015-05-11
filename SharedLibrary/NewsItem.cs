using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HorseMobileService.DataObjects
{
    /// <summary>
    /// Shared TableEntity NewsItems, since it's used in both MobileServices and WebJobs.
    /// </summary>
    public class NewsItem : TableEntity
    {
        public string Author_id { get; set; }
        public string Author_name { get; set; }
        public string Author_pic_url { get; set; }
        public string Text { get; set; }
        public string Pic_url { get; set; }
        public DateTime PublishTime { get; set; }
        public string Horse_id { get; set; }
        public string Horse_name { get; set; }
        public int Comment_Count { get; set; }
        public int Like_Count { get; set; }
        public bool IsReady { get; set; }
    }
}
