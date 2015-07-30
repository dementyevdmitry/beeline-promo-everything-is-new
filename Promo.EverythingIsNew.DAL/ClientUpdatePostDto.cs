using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promo.EverythingIsNew.DAL
{
    public class ClientUpdatePostDto
    {
        public string ctn { get; set; } // String(10)  поле обязательно, Номер абонента в сети Билайн
        public string name { get; set; } // String(50)  Нет Имя абонента
        public string surname { get; set; } // String(100) Нет Фамилия абонента
        public string patronymic { get; set; } // String(50)  Нет Отчество абонента
        public string email { get; set; } // String(129) Нет Email адрес абонента
        public string birth_date { get; set; } // Date    Нет Дата рождения абонента
        public string email_unsubscribe { get; set; } // boolean Нет Отписка абонента от рассылки.Если true, тогда стоит отказ от рассылки.
    }
}
