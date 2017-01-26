using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLC;

namespace ChatManager
{
    public class QueryBuilder
    {
        #region Get Message
        protected async Task<Message> _GetMessage(int RoomId, int MessageId)
        {
            var query = $"SELECT * FROM MESSAGE WHERE message_id = {MessageId} AND chat_room_id = {RoomId};";

            try
            {
                using (var _Connection = Connection.GetConnection())
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

                            return null;
                        };
                    }
                }
            }
            catch
            {
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
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new SqlCommand(query, _Connection))
                    {

                        await _Connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var listOfMessages = new List<Message>();

                            while (await reader.ReadAsync())
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

                            return listOfMessages;
                        }
                    }
                };
            }
            catch
            {
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
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new SqlCommand(query, _Connection))
                    {
                        await _Connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                };

            }
            catch
            {
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
                query += $"UPDATE CHAT_ROOM SET users = '{GetTextFromList(Users, "userid")}' WHERE chat_room_id = {ChatRoom.Id};";
            }
            if (Owner != null)
            {
                query += $"UPDATE CHAT_ROOM SET owner = '{Owner.userid}' WHERE chat_room_id = {ChatRoom.Id};";
            }

            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new SqlCommand(query, _Connection))
                    {
                        await _Connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        return new ChatRoom()
                        {
                            Id = ChatRoom.Id,
                            Name = Name ?? ChatRoom.Name,
                            Owner = Owner ?? ChatRoom.Owner,
                            Users = Users ?? ChatRoom.Users
                        };
                    }
                };
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Create Chat Room
        protected async Task<ChatRoom> _CreateChatRoom(string Name, List<User> Users, User Owner)
        {
            var query = $"INSERT INTO CHAT_ROOM(room_name, users, owner) VALUES ('{Name}', '";
            query += GetTextFromList<User>(Users, "userid");
            query += $"', {Owner.userid}); SELECT TOP 1 chat_room_id FROM CHAT_ROOM ORDER BY chat_room_id DESC;";

            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new SqlCommand(query, _Connection))
                    {
                        await _Connection.OpenAsync();

                        var chatRoom = new ChatRoom()
                        {
                            Id = (int)(await command.ExecuteScalarAsync()),
                            Name = Name,
                            Owner = Owner,
                            Users = Users
                        };
                        query = "";
                        foreach (var user in chatRoom.Users)
                        {
                            query += $"UPDATE USERS SET messages = (convert(nvarchar(max), (SELECT messages FROM USERS WHERE userid = {user.userid})) + '{chatRoom.Id}#') WHERE userid = {user.userid}; ";
                        }

                        command.CommandText = query;
                        await command.ExecuteNonQueryAsync();

                        return chatRoom;
                    }
                };
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Get user Chat rooms
        protected async Task<List<ChatRoom>> _GetUserChatRooms(User user)
        {
            try
            {
                return await _GetMultiListFromIdies<ChatRoom>(GetListOfIdFromText(user.messages));
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Updating User Information

        protected async Task<bool> _RemoveUserFromChatRoom(User User, ChatRoom ChatRoom)
        {
            var userChatSequence = GetIDSequence(GetListOfIdFromText(User.messages)).Replace($"{ChatRoom.Id}#", "");
            var chatUserSequence = GetIDSequence(GetListOfIdFromText(GetTextFromList<User>(ChatRoom.Users, "userid")));

            string query = $"UPDATE USERS SET messages = '{userChatSequence}' WHERE userid = {User.userid};";
            query += $"UPDATE CHAT_ROOM SET users = '{chatUserSequence} WHEREs chat_room_id = {ChatRoom.Id};";

            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new SqlCommand(query, _Connection))
                    {
                        await _Connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        protected async Task<bool> _UpdateUserInformation(int UserId, List<ChatRoom> ChatRooms = null)
        {
            string query = string.Empty;

            if (ChatRooms != null)
            {
                query += $"UPDATE USERS SET messages = '{GetTextFromList(ChatRooms, "Id")} WHERE userid = {UserId};";
            }

            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new SqlCommand(query, _Connection))
                    {
                        await _Connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                };
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Get Chat Room
        private string GetTextFromList<T>(List<T> List, string PropetyName)
        {
            string result = string.Empty;

            int[] idies = new int[List.Count];

            foreach (var id in List.Select(p => p.GetType().GetProperty(PropetyName).GetValue(p, null)))
            {
                result += $"{id}#";
            }

            return result;
        }

        protected IEnumerable<int> GetListOfIdFromText(string Text)
        {
            return Text.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries).Select(p => int.Parse(p));
        }

        private async Task<List<T>> GetListFromTextId<T>(string Text)
        {
            var usersId = Text.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries).Select(p => int.Parse(p));

            List<T> list = new List<T>();

            foreach (int userId in usersId)
            {
                if (typeof(T) == typeof(User))
                {
                    list.Add((T)(object)(await _GetUser(userId)));
                }
                else if (typeof(T) == typeof(ChatRoom))
                {
                    list.Add((T)(object)(await _GetChatRoom(userId)));
                }
            }

            return list;
        }

        #region Get Multi List from List of ID
        protected string GetIDSequence(IEnumerable<int> List)
        {
            string outText = string.Empty;
            foreach (var id in List)
            {
                outText += $"{id},";
            }
            return string.Concat(outText.Take(outText.Length - 1));
        }
        protected async Task<List<T>> _GetMultiListFromIdies<T>(IEnumerable<int> ListOfId, int UserId = -1, bool NewList = false)
        {
            string query = string.Empty;

            List<T> returnList = new List<T>();

            bool IsChatRoom = false;
            if (typeof(T) == typeof(ChatRoom))
            {
                IsChatRoom = true;
                if (NewList == false)
                {
                    query = $"SELECT * FROM CHAT_ROOM WHERE chat_room_id IN ({GetIDSequence(ListOfId)});";
                }
                else
                {
                    query = "DECLARE @string VARCHAR(512);";
                    query += "DECLARE @SQL VARCHAR(1024);";
                    query += $"SET @string = REPLACE((SELECT messages FROM USERS WHERE userid = {UserId}), '#', ', ' );";
                    query += " if len(@string) = 0";
                    query += " BEGIN";
                    query += " SELECT '' as nothing;";
                    query += " END";
                    query += " ELSE";
                    query += " BEGIN";
                    query += " SET @string = left(@string, len(@string) - 1);";
                    query += " SET @SQL = 'SELECT * FROM CHAT_ROOM WHERE chat_room_id IN(' + (@string) + ');';";
                    query += " EXECUTE(@SQL);";
                    query += " END";
                }
            }
            else if (typeof(T) == typeof(User))
            {
                query = $"SELECT * FROM USERS WHERE userid IN ({GetIDSequence(ListOfId)});";
            }

            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new SqlCommand(query, _Connection))
                    {
                        await _Connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                if (!reader.HasRows || (reader.HasRows && reader.GetName(0) == "nothing"))
                                {
                                    return returnList;
                                }

                                if (IsChatRoom)
                                {
                                    var chatRoom = new ChatRoom()
                                    {
                                        Id = reader.GetInt32(0),
                                        Name = reader.GetString(1),
                                        Owner = await _GetUser(reader.GetInt32(3)),
                                        Users = await GetListFromTextId<User>(reader.GetString(2)),
                                    };

                                    returnList.Add((T)(object)chatRoom);
                                }
                                else
                                {
                                    var user = new User()
                                    {
                                        userid = reader.GetInt32(0),
                                        login = reader.GetString(1),
                                        email = reader.GetString(3),
                                        usertype = reader.GetInt32(4),
                                        lastactivity = reader.GetInt32(5),
                                        regtime = reader.GetInt32(6),
                                        messages = $"{reader.GetValue(8)}",
                                        userteams = $"{reader.GetValue(9)}"
                                    };

                                    returnList.Add((T)(object)user);
                                }
                            }
                        }
                    }

                    return returnList;
                };
            }
            catch
            {
                return null;
            }
        }
        #endregion

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
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new SqlCommand(query, _Connection))
                    {
                        await _Connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var chatRoom = new ChatRoom()
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Owner = await _GetUser(reader.GetInt32(3)),
                                    Users = await GetListFromTextId<User>(reader.GetString(2))
                                };


                                return chatRoom;
                            }

                            return null;
                        }
                    }
                };
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Get User
        protected async Task<User> _GetUser(int Id = -1, string Login = "", bool GetLastMessage = false)
        {
            var query = $"SELECT * FROM USERS WHERE ";

            if (Id != -1)
            {
                query += $"userid  = {Id};";
            }
            else
            {
                query += $"login  = '{Login};";
            }

            if(GetLastMessage == true)
            {
                query += "SELECT TOP 1 message_id FROM MESSAGE ORDER BY message_id DESC;";
            }

            try
            {

                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new SqlCommand(query, _Connection))
                    {
                        await _Connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            User user = new User();

                            while (await reader.ReadAsync())
                            {
                                user = new User()
                                {
                                    userid = reader.GetInt32(0),
                                    login = reader.GetString(1),
                                    email = reader.GetString(3),
                                    usertype = reader.GetInt32(4),
                                    lastactivity = reader.GetInt32(5),
                                    regtime = reader.GetInt32(6),
                                    messages = $"{reader.GetValue(8)}",
                                    userteams = $"{reader.GetValue(9)}"
                                };
                                break;
                            }

                            if(GetLastMessage && await reader.NextResultAsync())
                            {
                                while(await reader.ReadAsync())
                                {
                                    user._LastMessageId = reader.GetInt32(0);
                                    break;
                                }
                            }
                            return user;
                        }
                    }
                };
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }
}
