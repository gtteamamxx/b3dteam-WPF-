using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace b3dteam.Model
{
    public class Ball3DProcess
    {
        public const double CHECK_STATUS_DURATION = 0.2; // In minutes
        public MainWindow MainWindowInstance;

        public Ball3DProcess(MainWindow instance)
        {
            MainWindowInstance = instance;
        }
        public void RunGame()
        {
            using (Process process = Process.Start(Properties.Settings.Default.Ball3DExePath))
            {
                CheckBall3DProcessAndSendStatus();
            };
        }

        public async void CheckBall3DProcessAndSendStatus()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(CHECK_STATUS_DURATION));

                if (IsBall3DProcessRunning())
                {
                    if (Ball3DStatus.ClientStatus == Ball3DStatus.Ball3D_Status.Status_Online)
                    {
                        Ball3DStatus.UpdateStatus(Ball3DStatus.Ball3D_Status.Status_Online);
                    }
                }
                else
                {
                    MainWindowInstance.ExitApplicationWithOffineStatus();
                    break;
                }
            }
        }

        public bool IsBall3DProcessRunning()
        {
            try
            {
                if (Process.GetProcessesByName("Ball 3D").Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
