using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace b3dteam_app.Model
{
    public class Message
    {
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public string Author { get; set; }
        public string ShortTime => Time.ToShortTimeString();
    }
}
