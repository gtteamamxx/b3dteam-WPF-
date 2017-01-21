using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatManager
{
    public class User
    {
        public int userid { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public int usertype { get; set; }
        public int lastactivity { get; set; }
        public int regtime { get; set; }
        public string userfriends { get; set; }
        public string messages { get; set; }
        public string userteams { get; set; }
    }
}
