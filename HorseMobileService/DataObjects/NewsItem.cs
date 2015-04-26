using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HorseMobileService.DataObjects
{
    public class NewsItem : TableEntity
    {
        public string Author_id { get; set; }
        public string Author_name { get; set; }
        public string Author_pic_url { get; set; }
        public string Text { get; set; }
        public string Pic_url { get; set; }
        public DateTime PublishTime { get; set; }
        public string Horse_id { get; set; }
    }
}
