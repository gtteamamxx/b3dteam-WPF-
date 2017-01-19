using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helper
{
    public class User
    {
        public delegate void _ClientStatusChanged(SQLManager.Ball3D_Status newStatus, SQLManager.Ball3D_Status oldStatus);
        public static event _ClientStatusChanged OnClientStatusChanged;

        public delegate void _ClientSave(User user);
        public static event _ClientSave OnClientSave;

        public delegate void _ClientReset(User user);
        public static event _ClientReset OnClientReset;

        private static User _ClientUser;
        public static SQLManager.Ball3D_Status _ClientStatus;
        
        public static User ClientUser
        {
            get
            {
                return _ClientUser;
            }
            set
            {
                _ClientUser = value;
            }
        }
        public static SQLManager.Ball3D_Status ClientStatus
        {
            get
            {
                return _ClientStatus;
            }
            set
            {
                var oldStatus = _ClientStatus;
                _ClientStatus = value;
                OnClientStatusChanged?.Invoke(value, oldStatus);
            }
        }

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
        public int autologin { get; set; }
        public int rememberme { get; set; }

        public void _setUser(int userid, string login, string password, string email, int usertype)
        {
            this.userid = userid;
            this.login = login;
            this.password = password;
            this.email = email;
            this.usertype = usertype;

            ClientUser = this;
        }

        public User()
        {
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
        public User(int userid, string login, string password, string email, int lastactivity, int regtime, int usertype, string userfriends)
        {
            _setUser(userid, login, password, email, usertype);
            this.lastactivity = lastactivity;
            this.regtime = regtime;
            this.userfriends = userfriends;
        }
        public User(int userid, string login, string password, string email, int lastactivity, int regtime, int usertype, string userfriends, string messages)
        {
            _setUser(userid, login, password, email, usertype);
            this.lastactivity = lastactivity;
            this.regtime = regtime;
            this.userfriends = userfriends;
            this.messages = messages;
        }
        public User(int userid, string login, string password, string email, int lastactivity, int regtime, int usertype, string userfriends, string messages, string userteams)
        {
            _setUser(userid, login, password, email, usertype);
            this.lastactivity = lastactivity;
            this.regtime = regtime;
            this.userfriends = userfriends;
            this.messages = messages;
            this.userteams = userteams;
        }

        public static void Save()
        {
            OnClientSave?.Invoke(_ClientUser);
        }

        public static void Reset()
        {
            OnClientReset?.Invoke(_ClientUser);
        }

        public SQLManager.Ball3D_Status GetuserStatus()
        {
            var now = SQLManager.GetTimeStamp();
            //idle for 3 minutes is provided as offine
            return (now - this.lastactivity > 3 * 60) ? SQLManager.Ball3D_Status.Status_Offine : SQLManager.Ball3D_Status.Status_Online;
        }

        public string GetUserAccountTypeName()
        {
            switch(this.usertype)
            {
                case 0:
                    return "Account not activated";
                case 1:
                    return "User";
                case 2:
                    return "Moderator";
                case 3:
                    return "Administartor";
                case 4:
                    return "Author of this program";
                case 5:
                    return "Author of the game";
                case 6:
                    return "Team captain";
                case 7:
                    return "Streamer";
                case 8:
                    return "Special person";
                default:
                    return "Unkown account";
            }
        }

        public DateTime GetDateTimeFromLatActivity()
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dtDateTime.AddSeconds(this.lastactivity).ToLocalTime();
        }
    }
}
