using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace b3dteam_app.Model
{
    public class Server
    {
        public string Name{ get; set; }
        public ulong Id { get; set; }
        public string MuteText { get; set; }
        public SolidColorBrush MuteButtonColor => MuteText.Contains("Un") ? 
                    new SolidColorBrush(new System.Windows.Media.Color { A = 50, R = 255 }) : 
                    new SolidColorBrush(new System.Windows.Media.Color { A = 50, G = 255 });

    }
}
