using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace b3dteam
{
    public static class Network
    {
        public static async Task<bool> IsInternetAvailable()
        {
            WebClient client = new WebClient();

            try
            {
                using (await client.OpenReadTaskAsync("http://google.pl"))
                {
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}