using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatManager
{
    public class QueryBuilder
    {
        #region Init
        private SqlConnection _Connection;

        protected QueryBuilder(SqlConnection connection)
        {
            this._Connection = connection;
        }
        public QueryBuilder() { }
        #endregion

        #region Get Message
        protected async Task<Message> _GetMessage(int RoomId, int MessageId)
        {
            var query = $"SELECT * FROM MESSAGE WHERE message_id = {MessageId} AND chat_room_id = {RoomId};";

            try
            {
                using (var command = new SqlCommand(query, _Connection))
                {
                    await _Connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var listOfMessages = new List<Message>();

                        while (await reader.ReadAsync())
                        {
                            return new Message()
                            {
                                message_id = reader.GetInt32(0),
                                chat_room_id = reader.GetInt32(1),
                                message = reader.GetString(2),
                                owner = await _GetUser(reader.GetInt32(3)),
                                timestamp = reader.GetInt32(4)
                            };
                        }

                        _Connection.Close();
                        return null;
                    }
                }
            }
            catch
            {
                _Connection.Close();
                return null;
            }
        }
        #endregion

        #region Get Messages
        protected async Task<List<Message>> _GetMessages(int RoomId, int Limit = 100)
        {
            var query = $"SELECT TOP {Limit} * FROM MESSAGE WHERE chat_room_id = {RoomId} ORDER BY message_id DESC;";

            try
            {
                using (var command = new SqlCommand(query, _Connection))
                {
                    await _Connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var listOfMessages = new List<Message>();

                        while(await reader.ReadAsync())
                        {
                            listOfMessages.Add(new Message()
                            {
                                message_id = reader.GetInt32(0),
                                chat_room_id = reader.GetInt32(1),
                                message = reader.GetString(2),
                                owner = await _GetUser(reader.GetInt32(3)),
                                timestamp = reader.GetInt32(4)
                            });
                        }

                        _Connection.Close();

                        return listOfMessages;
                    }
                }
            }
            catch
            {
                _Connection.Close();
                return null;
            }
        }
        #endregion

        #region Send Message
        protected async Task<bool> _SendMessage(int RoomId, int SenderUserId, string Text)
        {
            var query = $"INSERT INTO MESSAGE(chat_room_id, message, owner, timestamp) VALUES ({RoomId}, '{Text}', {SenderUserId}, {Chat.GetTimeStamp()});";
            try
            {
                using (var command = new SqlCommand(query, _Connection))
                {
                    await _Connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _Connection.Close();

                    return true;
                }
            }
            catch
            {
                _Connection.Close();
                return false;
            }
        }
        #endregion

        #region Update Chat Room
        protected async Task<ChatRoom> _UpdateChatRoom(ChatRoom ChatRoom, string Name = null, List<User> Users = null, User Owner = null)
        {
            string query = "";

            if (Name != null)
            {
                query += $"UPDATE CHAT_ROOM SET room_name = '{Name}' WHERE chat_room_id = {ChatRoom.Id};";
            }
            if (Users != null)
            {
                query += $"UPDATE CHAT_ROOM SET users = '{GetTextFromUsers(Users)}' WHERE chat_room_id = {ChatRoom.Id};";
            }
            if (Owner != null)
            {
                query += $"UPDATE CHAT_ROOM SET owner = '{Owner.userid}' WHERE chat_room_id = {ChatRoom.Id};";
            }

            try
            {
                using (var command = new SqlCommand(query, _Connection))
                {
                    await _Connection.OpenAsync();

                    await command.ExecuteNonQueryAsync();

                    _Connection.Close();

                    return new ChatRoom()
                    {
                        Id = ChatRoom.Id,
                        Name = Name ?? ChatRoom.Name,
                        Users = Users ?? ChatRoom.Users,
                        Owner = Owner ?? ChatRoom.Owner
                    };
                }
            }
            catch
            {
                _Connection.Close();
                return null;
            }
        }
        #endregion

        #region Create Chat Room
        protected async Task<ChatRoom> _CreateChatRoom(string Name, List<User> Users, User Owner)
        {
            var query = $"INSERT INTO CHAT_ROOM(room_name, users, owner) VALUES ('{Name}', '";
            query += GetTextFromUsers(Users);
            query += $"', {Owner.userid}); SELECT TOP 1 chat_room_id FROM CHAT_ROOM ORDER BY chat_room_id DESC;";

            try
            {
                using (var command = new SqlCommand(query, _Connection))
                {
                    await _Connection.OpenAsync();

                    var chatRoom = new ChatRoom()
                    {
                        Id = (int)(await command.ExecuteScalarAsync()),
                        Name = Name,
                        Users = Users,
                        Owner = Owner
                    };
                        
                    _Connection.Close();

                    return chatRoom;
                }
            }
            catch
            {
                _Connection.Close();
                return null;
            }
        }
        #endregion

        #region Get Chat Room

        private string GetTextFromUsers(List<User> Users, User Owner = null)
        {
            string result = string.Empty;

            int[] usersId = new int[Users.Count];

            foreach (var id in Users.Select(p => p.userid))
            {
                result += $"{id}#";
            }

            return result;
        }
        private async Task<List<User>> GetUsersFromText(string Text)
        {
            var usersId = Text.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries).Select(p => int.Parse(p));

            List<User> users = new List<User>();

            foreach (int userId in usersId)
            {
                users.Add(await _GetUser(userId));
            }

            return users;
        }

        protected async Task<ChatRoom> _GetChatRoom(int Id = -1, string Name = "")
        {
            var query = $"SELECT * FROM CHAT_ROOM WHERE ";

            if (Id != -1)
            {
                query += $"chat_room_id = { Id};";
            }
            else
            {
                query += $"room_name = '{Name};";
            }

            try
            {
                using (var command = new SqlCommand(query, _Connection))
                {
                    await _Connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                        {
                            var chatRoom = new ChatRoom()
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Users = await GetUsersFromText(reader.GetString(2)),
                                Owner = await _GetUser(reader.GetInt32(3))
                            };

                            _Connection.Close();

                            return chatRoom;
                        }

                        _Connection.Close();
                        return null;
                    }
                }
            }
            catch
            {
                _Connection.Close();
                return null;
            }
        }

        #endregion

        #region Get User
        protected async Task<User> _GetUser(int Id = -1, string Login = "")
        {

            var query = $"SELECT * FROM USERS WHERE ";

            if (Id != -1)
            {
                query += $"userid  = { Id};";
            }
            else
            {
                query += $"login  = '{Login};";
            }

            try
            {
                using (var command = new SqlCommand(query, _Connection))
                {
                    await _Connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                        {
                            var user = new User()
                            {
                                userid = reader.GetInt32(0),
                                login = reader.GetString(1),
                                email = reader.GetString(3),
                                usertype = reader.GetInt32(4),
                                lastactivity = reader.GetInt32(5),
                                regtime = reader.GetInt32(6),
                                userfriends = $"{reader.GetValue(7)}",
                                messages = $"{reader.GetValue(8)}",
                                userteams = $"{reader.GetValue(9)}"
                            };

                            _Connection.Close();

                            return user;
                        }
                    }

                    _Connection.Close();
                    return null;
                }
            }
            catch
            {
                _Connection.Close();
                return null;
            }
        }
        #endregion
    }
}
