using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace b3dteam.Model
{
    public static class Extension
    {
        public static void SaveUser(this helper.User user)
        {
            Properties.Settings.Default.email = user.email;
            Properties.Settings.Default.login = user.login;
            Properties.Settings.Default.password = user.password;
            Properties.Settings.Default.userid = user.userid;
            Properties.Settings.Default.usertype = user.usertype;
            Properties.Settings.Default.regtime = user.regtime;
            Properties.Settings.Default.lastactivity = user.lastactivity;
            Properties.Settings.Default.autologin = (user.autologin == 1 ? true : false);
            Properties.Settings.Default.rememberme = (user.rememberme == 1 ? true : false);
            Properties.Settings.Default.Save();
        }

        public static void ResetUser(this helper.User user)
        {
            MainWindow.ClientUser = null;

            Properties.Settings.Default.email = "";
            Properties.Settings.Default.login = "";
            Properties.Settings.Default.password = "";
            Properties.Settings.Default.userid = -1;
            Properties.Settings.Default.usertype = -1;
            Properties.Settings.Default.regtime = 0;
            Properties.Settings.Default.lastactivity = 0;
            Properties.Settings.Default.autologin = false;
            Properties.Settings.Default.rememberme = false;
            Properties.Settings.Default.Save();
        }
    }
}
