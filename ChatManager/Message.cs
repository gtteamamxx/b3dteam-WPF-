using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatManager
{
    public class Message
    {
        public int message_id { get; protected internal set; }
        public int chat_room_id { get; protected internal set; }
        public string message { get; protected internal set; }
        public User owner { get; protected internal set; }
        public int timestamp { get; protected internal set; }
    }
}
