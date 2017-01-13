using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helper
{
    public static class Application
    {
        public static bool IsAppRunning = false;
        public delegate void CloseAppEvent();
        public static event CloseAppEvent OnCloseApp;

        public static void CloseApp()
        {
            OnCloseApp?.Invoke();
        }
    }
}
