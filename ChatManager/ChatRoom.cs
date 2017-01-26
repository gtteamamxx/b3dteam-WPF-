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
        public enum RoomChangeType
        {
            Deleted,
            New
        }

        public int Id { get; protected internal set; }
        public string Name { get; protected internal set; }
        private List<User> _allusers { get; set; }
        public List<User> Users { get { return _allusers; } protected internal set
            {
                _allusers = value;
                _allusers.Add(this.Owner);
            } }
        public User Owner { get; protected internal set; }

        public async Task<bool> SendMessage(int SenderUserIdr, string message)
        {
            return await _SendMessage(this.Id, SenderUserIdr, message);
        }

        public async Task<List<Message>> GetMessages(int Limit = 25)
        {
            return await _GetMessages(this.Id, Limit);
        }

        public bool UserHasAccesToChannel(User user)
        {
            return (user.userid == Owner.userid || Users.FirstOrDefault(p => p.userid == user.userid) != null);
        }
    }
}
