using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace b3dteam.Model
{
    public class User
    {
        public int userid { get; set; }
        public string login { get; set; }
        public string password { get; set; }

        public User(int userid, string login, string password)
        {
            this.userid = userid;
            this.login = login;
            this.password = password;
        }
    }
}
