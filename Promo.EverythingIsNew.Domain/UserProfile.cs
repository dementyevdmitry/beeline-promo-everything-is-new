using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltLanDS.AllNew.Core
{
    public class UserProfile
    {
        public string UserID { get; set; }  
        public string City { get; set; }
        public string FIO { get; set; }  // Имя, фамилия
        public DateTime? Birthday {get;set;}
        public string Academy { get; set; }
        public string Email { get; set; }
    
        
    }
}
