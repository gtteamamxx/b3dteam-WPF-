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

        public void Save()
        {
            Properties.Settings.Default.email = email;
            Properties.Settings.Default.login = login;
            Properties.Settings.Default.password = password;
            Properties.Settings.Default.userid = userid;
            Properties.Settings.Default.usertype = usertype;
            Properties.Settings.Default.regtime = regtime;
            Properties.Settings.Default.lastactivity = lastactivity;

            Properties.Settings.Default.Save();
        }

        public void Reset()
        {
            Properties.Settings.Default.email = "";
            Properties.Settings.Default.login = "";
            Properties.Settings.Default.password = "";
            Properties.Settings.Default.userid = -1;
            Properties.Settings.Default.usertype = -1;
            Properties.Settings.Default.regtime = 0;
            Properties.Settings.Default.lastactivity = 0;

            Properties.Settings.Default.Save();
        }
    }
}
