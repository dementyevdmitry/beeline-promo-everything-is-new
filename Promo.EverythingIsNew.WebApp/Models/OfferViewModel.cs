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

        public string EveryMontMinutesPackage { get; set; }
        public string EveryMontMinutesRegion { get; set; }

        public string EveryMontGbPackage { get; set; }
        public string EveryMontGbRegion { get; set; }

        public string EveryMontSmsPackage { get; set; }
        public string EveryMontSmsRegion { get; set; }

        public string SubscriptionFee { get; set; }
        public string TransitionCost { get; set; }
    }
}