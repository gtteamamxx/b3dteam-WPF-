using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLC;

namespace ChatManager
{
    public class QueryBuilder
    {
        private List<User> _TempListOfUsers;
        private List<ChatRoom> _TempListOfChatRooms;
        private List<Message> _TempListOfMessages;

        public QueryBuilder()
        {
            _TempListOfUsers = new List<User>();
            _TempListOfChatRooms = new List<ChatRoom>();
            _TempListOfMessages = new List<Message>();
        }

        #region Get Message
        protected async Task<Message> _GetMessage(int RoomId, int MessageId)
        {
            var tempResult = _TempListOfMessages.FirstOrDefault(p => p.chat_room_id == RoomId && p.message_id == MessageId);
            if(tempResult != null)
            {
                return tempResult;
            }

            var query = $"SELECT * FROM MESSAGES WHERE message_id = {MessageId} AND chat_room_id = {RoomId};";

            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new MySqlCommand(query, _Connection))
                    {
                        await _Connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var listOfMessages = new List<Message>();

                            while (await reader.ReadAsync())
                            {
                                var message = new Message()
                                {
                                    message_id = reader.GetInt32(0),
                                    chat_room_id = reader.GetInt32(1),
                                    message = reader.GetString(2),
                                    ownerId = reader.GetInt32(3),
                                    ownerName = reader.GetString(4),
                                    timestamp = reader.GetInt32(5)
                                };

                                if(!_TempListOfMessages.Any(p => p.message_id == message.message_id))
                                {
                                    _TempListOfMessages.Add(message);
                                }
                                return message;
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
            var query = $"SELECT * FROM ( SELECT * FROM MESSAGES WHERE chat_room_id = {RoomId} ORDER BY message_id DESC LIMIT {Limit})  AS T ORDER BY message_id; ";

            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new MySqlCommand(query, _Connection))
                    {
                        await _Connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var listOfMessages = new List<Message>();

                            while (await reader.ReadAsync())
                            {
                                var message = new Message()
                                {
                                    message_id = reader.GetInt32(0),
                                    chat_room_id = reader.GetInt32(1),
                                    message = reader.GetString(2),
                                    ownerId = reader.GetInt32(3),
                                    ownerName = reader.GetString(4),
                                    timestamp = reader.GetInt32(5)
                                };

                                listOfMessages.Add(message);

                                if (!_TempListOfMessages.Any(p => p.message_id == message.message_id))
                                {
                                    _TempListOfMessages.Add(message);
                                }
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
        protected async Task<Message> _SendMessage(int RoomId, User user, string Text)
        {
            var query = $"INSERT INTO MESSAGES(chat_room_id, message, owner, timestamp, owner_name) VALUES ({RoomId}, '{Text}', {user.userid}, {Chat.GetTimeStamp()}, '{user.login}'); SELECT * FROM MESSAGES ORDER BY message_id DESC LIMIT 1;";
            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new MySqlCommand(query, _Connection))
                    {
                        await _Connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var listOfMessages = new List<Message>();

                            while (await reader.ReadAsync())
                            {
                                var message = new Message()
                                {
                                    message_id = reader.GetInt32(0),
                                    chat_room_id = reader.GetInt32(1),
                                    message = reader.GetString(2),
                                    ownerId = reader.GetInt32(3),
                                    ownerName = reader.GetString(4),
                                    timestamp = reader.GetInt32(5)
                                };

                                if (!_TempListOfMessages.Any(p => p.message_id == message.message_id))
                                {
                                    _TempListOfMessages.Add(message);
                                }

                                return message;
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
                query += $" UPDATE CHAT_ROOM SET users = '{GetTextFromList(Users, "userid")}' WHERE chat_room_id = {ChatRoom.Id};";
            }
            if (Owner != null)
            {
                query += $" UPDATE CHAT_ROOM SET owner = '{Owner.userid}' WHERE chat_room_id = {ChatRoom.Id};";
            }

            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new MySqlCommand(query, _Connection))
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
            var query = $"INSERT INTO CHAT_ROOM(room_name, users, owner, owner_name) VALUES ('{Name}', '";
            query += GetTextFromList<User>(Users, "userid");
            query += $"', {Owner.userid}, '{Owner.login}'); SELECT `chat_room_id` FROM `CHAT_ROOM` ORDER BY `chat_room_id` DESC LIMIT 1;";

            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new MySqlCommand(query, _Connection))
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
                            query += $" SET @temp = CONCAT( (SELECT `messages` FROM `USERS` WHERE `userid` = {user.userid} ) , '{chatRoom.Id}#'); ";
                            query += $"UPDATE `USERS` SET `messages` = @temp WHERE `userid` = {user.userid};";
                        }

                        command.CommandText = query;
                        await command.ExecuteNonQueryAsync();

                        if (!_TempListOfChatRooms.Any(p => p.Id == chatRoom.Id))
                        {
                            _TempListOfChatRooms.Add(chatRoom);
                        }
                        
                        return chatRoom;
                    }
                };
            }
            catch(Exception ex)
            {
                    ;
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
            var userChatSequence = User.messages.Replace($"{ChatRoom.Id}#", "");

            string query = $@"
            UPDATE USERS SET messages = '{userChatSequence}' WHERE userid = {User.userid};
            SET @v_string = TRIM(REPLACE((SELECT users FROM CHAT_ROOM WHERE chat_room_id = {ChatRoom.Id}), '{User.userid}#', ''));
            SET @v_sql = CONCAT('UPDATE CHAT_ROOM SET users = ', '''', @v_string, ''' WHERE chat_room_id = {ChatRoom.Id};');
            PREPARE sqlquery FROM @v_sql;
            EXECUTE sqlquery;".Replace("\r\n", "");

            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new MySqlCommand(query, _Connection))
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
                    using (var command = new MySqlCommand(query, _Connection))
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
        private string GetTextFromList<T>(IEnumerable<T> List, string PropetyName)
        {
            string result = string.Empty;

            int[] idies = new int[List.Count()];

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

        protected async Task<List<T>> GetListFromTextId<T>(string Text)
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

        protected string _GetQueryStringForChatRoomsOfUser(int UserId)
        {
            string  query = $@"
             DROP PROCEDURE IF EXISTS GETUSERCHATROOMS;
            CREATE PROCEDURE GETUSERCHATROOMS(out v_sql VARCHAR(512))
            BEGIN
                SET @v_string = TRIM(REPLACE((SELECT messages FROM USERS WHERE userid = {UserId}), '#', ', '));
            SET @v_len = CHAR_LENGTH(@v_string);
            IF @v_len = 0 THEN
                SET v_sql = 'SELECT '''' as nothing;';
            ELSE
                SET v_sql = CONCAT('SELECT * FROM CHAT_ROOM WHERE chat_room_id IN(', LEFT(@v_string, @v_len - 1), ');');
            END IF;
            END;
            CALL GETUSERCHATROOMS(@v_sqlquery);
            PREPARE v_query FROM @v_sqlquery;
            EXECUTE v_query; ";

            return query.Replace(Environment.NewLine, "");
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
                    query += _GetQueryStringForChatRoomsOfUser(UserId);
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
                    using (var command = new MySqlCommand(query, _Connection))
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
                                        OwnerId = reader.GetInt32(3),
                                        UsersString = reader.GetString(2),
                                    };
                                    if (!_TempListOfChatRooms.Any(p => p.Id == chatRoom.Id))
                                    {
                                        _TempListOfChatRooms.Add(chatRoom);
                                    }
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
                                        messages = $"{reader.GetValue(7)}",
                                        userteams = $"{reader.GetValue(8)}"
                                    };

                                    if(!_TempListOfUsers.Any(p => p.userid == user.userid))
                                    {
                                        _TempListOfUsers.Add(user);
                                    }
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
            var tempResult = _TempListOfChatRooms.FirstOrDefault(p => p.Id == Id);
            if (tempResult != null)
            {
                return tempResult;
            }

            var query = $"SELECT * FROM CHAT_ROOM WHERE ";

            if (Id != -1)
            {
                query += $"chat_room_id = { Id};";
            }
            else
            {
                query += $"strcmp(room_name, '{Name}');";
            }

            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new MySqlCommand(query, _Connection))
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
                                    OwnerId = reader.GetInt32(3),
                                    UsersString = reader.GetString(2)
                                };

                                if (!_TempListOfChatRooms.Any(p => p.Id == chatRoom.Id))
                                {
                                    _TempListOfChatRooms.Add(chatRoom);
                                }
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
            var tempResult = _TempListOfUsers.FirstOrDefault(p => p.userid == Id || p.login == Login);
            if (tempResult != null)
            {
                return tempResult;
            }

            var query = $"SELECT * FROM USERS WHERE ";

            if (Id != -1)
            {
                query += $"userid  = {Id};";
            }
            else
            {
                query += $"strcmp(login, '{Login}');";
            }

            if(GetLastMessage == true)
            {
                query += " SELECT message_id FROM MESSAGES ORDER BY message_id DESC LIMIT 1;";
            }

            try
            {
                using (var _Connection = Connection.GetConnection())
                {
                    using (var command = new MySqlCommand(query, _Connection))
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
                                    messages = $"{reader.GetValue(7)}",
                                    userteams = $"{reader.GetValue(8)}"
                                };
                                if (!_TempListOfUsers.Any(p => p.userid == user.userid))
                                {
                                    _TempListOfUsers.Add(user);
                                }
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

        protected DateTime _GetDateTimeFromTimeStamp(int timestamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dtDateTime.AddSeconds(timestamp).ToLocalTime();
        }
    }
}
