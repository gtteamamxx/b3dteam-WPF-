using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helper
{
    public class User
    {
        public static User _ClientUser;
        public static User ClientUser { get { return _ClientUser; } set { _ClientUser = value; } }

        public int userid { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public int usertype { get; set; }
        public int lastactivity { get; set; }
        public int regtime { get; set; }

        public void _setUser(int userid, string login, string password, string email, int usertype)
        {
            this.userid = userid;
            this.login = login;
            this.password = password;
            this.email = email;
            this.usertype = usertype;

            ClientUser = this;
        }

        public User(int userid, string login, string password, string email, int usertype)
        {
            _setUser(userid, login, password, email, usertype);
        }
        public User(int userid, string login, string password, string email, int lastactivity, int usertype)
        {
            _setUser(userid, login, password, email, usertype);
            this.lastactivity = lastactivity;
        }
        public User(int userid, string login, string password, string email, int lastactivity, int regtime, int usertype)
        {
            _setUser(userid, login, password, email, usertype);
            this.lastactivity = lastactivity;
            this.regtime = regtime;
        }
    }
}
