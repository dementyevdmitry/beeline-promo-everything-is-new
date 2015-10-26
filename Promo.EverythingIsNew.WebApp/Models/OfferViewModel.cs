using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Promo.EverythingIsNew.WebApp.Models
{
    public class OfferViewModel
    {
        public string UserName { get; set; }
        public string TariffName { get; set; }

        public string EverydayMinutesPackage { get; set; }
        public string EverydayTrafficMbPackage { get; set; }
        public string EverydaySmsPackage { get; set; }

        public string EveryMonthMinutesPackage { get; set; }
        public string EveryMonthMinutesRegion { get; set; }

        public string EveryMonthGbPackage { get; set; }
        public string EveryMonthGbRegion { get; set; }

        public string EveryMonthSmsPackage { get; set; }
        public string EveryMonthSmsRegion { get; set; }

        public string SubscriptionFee { get; set; }
        public string TransitionCost { get; set; }
    }
}