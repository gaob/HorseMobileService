using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HorseMobileService.DataObjects
{
    public class UserItem : EntityData
    {
        public string Name { get; set; }
        public string Pic_url { get; set; }
        public bool isAdmin { get; set; }
    }
}
