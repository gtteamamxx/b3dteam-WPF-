using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatManager
{
    public class ChatRoom : QueryBuilder
    {
        public int Id { get; protected internal set; }
        public string Name { get; protected internal set; }
        public List<User> Users { get; protected internal set; }
        public User Owner { get; protected internal set; }

        public async Task<bool> SendMessage(int SenderUserIdr, string message)
        {
            return await _SendMessage(this.Id, SenderUserIdr, message);
        }

        public async Task<List<Message>> GetMessages(int Limit = 25)
        {
            return await _GetMessages(this.Id, Limit);
        }
    }
}
