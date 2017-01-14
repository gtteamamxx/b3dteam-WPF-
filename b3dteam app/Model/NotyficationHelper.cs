using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace b3dteam_app.Model
{
    public static class NotyficationHelper
    {
        public delegate void SendMessageEvent(string title, string message);
        public static event SendMessageEvent OnSendMessage;

        public static void SendMessage(string title, string message)
        {
            OnSendMessage?.Invoke(title, message);
        }
    }
}
