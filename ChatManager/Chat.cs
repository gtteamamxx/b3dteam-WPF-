using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatManager
{
    public class Chat : QueryBuilder
    {

        public Chat()
        {
            SqlDependency.Start(SQLC.Connection.GetConnection().ConnectionString);
        }

        public async Task<ChatRoom> GetChatRoom(int Id = -1, string Name = "")
        {
            return await _GetChatRoom(Id, Name);
        }

        public async Task<User> GetUser(int Id = 1, string Login = "", bool GetLastMessage = false)
        {
            return await _GetUser(Id, Login, GetLastMessage);
        }

        public async Task<ChatRoom> CreateChatRoom(string Name, List<User> users, User owner)
        {
            return await _CreateChatRoom(Name, users, owner);
        }

        protected internal static int GetTimeStamp()
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}
