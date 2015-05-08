using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HorseMobileService.DataObjects
{
    public class NotificationItem : TableEntity
    {
        public string User_id { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
    }
}
