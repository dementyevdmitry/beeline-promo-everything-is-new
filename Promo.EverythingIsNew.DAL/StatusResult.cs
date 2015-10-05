using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promo.EverythingIsNew.DAL
{
    public class StatusResult
    {
        public bool is_beeline_subscriber { get; set; } // принадлежит ли номер абоненту билайн
        public bool is_used_uid { get; set; } // участвовал ли данный аккаунт в акции
        public bool is_used_ctn { get; set; } // участвовал ли данный абонент в акции

    }
}
