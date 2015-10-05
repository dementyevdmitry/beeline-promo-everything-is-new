using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promo.EverythingIsNew.DAL
{
    class UpdateResult

    {
        public bool status { get; set; }  // Boolean поле обязательно, Статус получения персональных данных. Если статус True. Тогда операция прошла успешно и ошибок не было обнаружено.
        public string description { get; set; }  // String(500)	Причина ошибки. Не пустое, если Status false. 
    }
}
