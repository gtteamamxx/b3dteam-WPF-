using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatManager
{
    public class Message : QueryBuilder
    {
        public int message_id { get; protected internal set; }
        public int chat_room_id { get; protected internal set; }
        public string message { get; protected internal set; }
        private User owner { get; set; }
        public int ownerId { get; protected internal set; }
        public string ownerName { get; protected internal set; }
        public int timestamp { get; protected internal set; }

        public async Task<User> GetOwner() => await _GetUser(ownerId);

        public DateTime GetDateTimeFromTimeStamp() => _GetDateTimeFromTimeStamp(this.timestamp);
        
    }
}
