using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HorseMobileService.DataObjects
{
    /// <summary>
    /// TableEntity for comment.
    /// </summary>
    public class CommentItem : TableEntity
    {
        public string Author_id { get; set; }
        public string Author_name { get; set; }
        public string Text { get; set; }
        public DateTime PublishTime { get; set; }
        public string News_id { get; set; }
        public bool Liked { get; set; }
    }
}