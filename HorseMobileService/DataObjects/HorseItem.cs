using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HorseMobileService.DataObjects
{
    public class HorseItem : EntityData
    {
        public string Name { get; set; }
        public string Pic_url { get; set; }
        public string Owner_id { get; set; }
        public string Gender { get; set; }
        public int Year { get; set; }
        public string Breed { get; set; }
        public string Registered { get; set; }
        public string Description { get; set; }
    }
}
