using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace b3dteam.Model
{
    public static class Ball3DStatus
    {
        public enum Ball3D_Status
        {
            Status_Offine = 0,
            Status_Online
        }

        public static Ball3D_Status ClientStatus = Ball3D_Status.Status_Offine;

        public static async void UpdateStatus(Ball3D_Status status)
        {
            var updateStatusSuccesful = await SQLManager.UpdateStatus(status, MainWindow.ClientUser.userid);

            if(updateStatusSuccesful == false)
            {
                int i = 0;
                for (; i < 2; i++)
                {
                    if(await SQLManager.UpdateStatus(status, MainWindow.ClientUser.userid))
                    {
                        i = 3;
                        break;
                    }
                }

                if(i != 3) //if two retries of update status failed
                {
                    NotyficationManager.MakeNotyfication("Error", "Error while sending an update status.");
                }
            }
        }
    }
}
