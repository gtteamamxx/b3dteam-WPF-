using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace b3dteam
{
    public static class NotyficationManager
    {
        private static MainWindow _InstanceOfMainWindow;

        public static void SetInstanceOfMainWindow(MainWindow instance)
        {
            _InstanceOfMainWindow = instance;

            b3dteam_app.Model.NotyficationHelper.OnSendMessage += (title, message) =>
            {
                MakeNotyfication(title, message);
            };
        }

        public static void MakeNotyfication(string title, string message)
        {
            if(_InstanceOfMainWindow == null)
            {
                throw new Exception("First set an instance!");
            }

            System.Windows.Forms.NotifyIcon icon = new System.Windows.Forms.NotifyIcon();
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Images/icon.ico")).Stream;
            icon.Icon = new System.Drawing.Icon(iconStream);

            _InstanceOfMainWindow.myNotifyIcon.ShowBalloonTip(title, message, icon.Icon);
        }
    }
}
