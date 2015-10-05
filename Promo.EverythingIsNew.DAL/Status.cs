using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promo.EverythingIsNew.DAL
{
    public class Status
    {
        public string ctn { get; set; } // String(10)	Обязательно:Да	Номер абонента в сети Билайн
        public string uid { get; set; } // String(20)	Идентификатор пользователя в системе, связанной с LP
    }
}
