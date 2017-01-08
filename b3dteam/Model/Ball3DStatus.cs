using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static helper.SQLManager;

namespace b3dteam.Model
{
    public static class Ball3DStatus
    {
        public static Ball3D_Status ClientStatus { get { return helper.User.ClientStatus; } set { helper.User.ClientStatus = value; } }

        public static async void UpdateStatus(Ball3D_Status status)
        {
            var updateStatusSuccesful = await helper.SQLManager.UpdateStatus(status, MainWindow.ClientUser.userid);

            if(updateStatusSuccesful == false)
            {
                int i = 0;
                for (; i < 2; i++)
                {
                    if(await helper.SQLManager.UpdateStatus(status, MainWindow.ClientUser.userid))
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
