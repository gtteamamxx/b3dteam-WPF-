using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace b3dteam.Model
{
    public static class Ball3DStatus
    {
        public  enum Ball3D_Status
        {
            Status_Online,
            Status_Offine
        }

        public static Ball3D_Status ClientStatus = Ball3D_Status.Status_Offine;

        public static void UpdateStatus(Ball3D_Status status)
        {

        }
    }
}
