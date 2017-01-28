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

        private List<User> _allusers { get; set; }

        public int Id { get; protected internal set; }
        public string Name { get; protected internal set; }
        public string UsersString { get; protected internal set; }
        public List<User> Users { get { return _allusers; } protected internal set { _allusers = value; _allusers.Add(this.Owner); } }
        public int OwnerId { get; protected internal set; }
        public User Owner { get; protected internal set; }

        public async Task<Message> SendMessage(User user, string message)
        {
            var msg = await _SendMessage(this.Id, user, message);

            if(msg != null)
            {
                user._ListOfsendedMessagesButNotFetched.Add(msg);
            }

            return msg;
        }

        public async Task<List<Message>> GetMessages(int Limit = 25) => await _GetMessages(this.Id, Limit);
        public async Task<User> GetOwner() => await _GetUser(this.OwnerId);
        public bool UserHasAccesToChannel(User user) =>_allusers.Any(p => p.userid == user.userid);
        
    }
}
