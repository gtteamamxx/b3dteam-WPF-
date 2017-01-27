using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatManager
{
    public class User : QueryBuilder
    {
        private static readonly double CHECK_MESSAGE_TIME = 5.0;

        public delegate void UserRoomsChanged(List<ChatRoom> ChatRoom, ChatRoom.RoomChangeType ChangeType);
        public delegate void MessageReceived(Message message);

        public event UserRoomsChanged OnUserRoomsChanged;
        public event MessageReceived OnMessageReceived;

        public int userid { get; set; }
        public string login { get; protected internal set; }
        public string password { get; protected internal set; }
        public string email { get; protected internal set; }
        public int usertype { get; protected internal set; }
        public int lastactivity { get; protected internal set; }
        public int regtime { get; protected internal set; }
        public string messages { get; protected internal set; }
        public string userteams { get; protected internal set; }

        internal protected List<ChatRoom> _usersChatRoom { get; set; }
        internal protected int _LastMessageId { get; set; }

        public User()
        {
            _usersChatRoom = new List<ChatRoom>();
        }

        public void StartListeningChanges()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;

            worker.DoWork += (s, e) =>
            {
                SqlCommand command = new SqlCommand();
                command.CommandType = System.Data.CommandType.Text;
                string query = string.Empty;
                while (true)
                {
                    Task.Delay(TimeSpan.FromSeconds(CHECK_MESSAGE_TIME)).GetAwaiter().GetResult();

                    using (var _Connection = SQLC.Connection.GetConnection())
                    {
                        command.Connection = _Connection;

                        query = $"SELECT messages FROM USERS WHERE userid = {this.userid};";

                        if (messages.Length > 0)
                        {
                            query += $"SELECT * FROM MESSAGE WHERE message_id > {this._LastMessageId} AND chat_room_id IN ({GetIDSequence(GetListOfIdFromText(this.messages))}) ORDER BY message_id DESC;";
                        }

                        int i = 0;

                        command.CommandText = query;

                        _Connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            string msg = string.Empty;
                            List<ChatRoom> listDeletedRooms = new List<ChatRoom>();
                            List<ChatRoom> listNewRooms = new List<ChatRoom>();
                            List<int> tempListWithIDOfNewRooms = new List<int>();

                            if (reader.Read())
                            {
                                msg = reader.GetString(0);
                            }

                            if (msg.Length > 0 && !string.Equals(this.messages, msg))
                            {
                                var lastRooms = GetListOfIdFromText(this.messages);
                                var newRooms = GetListOfIdFromText(msg);

                                this.messages = msg;

                                lastRooms.Where(p => !newRooms.Contains(p)).ToList().ForEach(p =>
                                {
                                    listDeletedRooms.Add(new ChatRoom() { Id = p });
                                    _usersChatRoom.Remove(_usersChatRoom.First(g => g.Id == p));
                                });

                                if (listDeletedRooms.Count > 0)
                                {
                                    worker.ReportProgress(0, listDeletedRooms);
                                }

                                tempListWithIDOfNewRooms = newRooms.Where(p => !lastRooms.Contains(p)).ToList();
                            }

                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    if (i == 0)
                                    {
                                        i++;
                                        _LastMessageId = reader.GetInt32(0);
                                    }

                                    var message = new Message()
                                    {
                                        message_id = reader.GetInt32(0),
                                        chat_room_id = reader.GetInt32(1),
                                        message = reader.GetString(2),
                                        ownerId = reader.GetInt32(3),
                                        timestamp = reader.GetInt32(4),
                                        ownerName = reader.GetString(5)
                                    };

                                    worker.ReportProgress(1, message);
                                }
                            }

                            if (listDeletedRooms.Count > 0)
                            {
                                worker.ReportProgress(0, listDeletedRooms);
                            }

                            if (tempListWithIDOfNewRooms.Count > 0)
                            {
                                listNewRooms = _GetMultiListFromIdies<ChatRoom>(tempListWithIDOfNewRooms).Result;
                                worker.ReportProgress(2, listNewRooms);
                            }
                        }
                    }
                }
            };

            worker.ProgressChanged += (s, e) =>
            {
                if (e.ProgressPercentage == 0)
                {
                    OnUserRoomsChanged(e.UserState as List<ChatRoom>, ChatRoom.RoomChangeType.Deleted);
                }
                else if (e.ProgressPercentage == 1)
                {
                    OnMessageReceived(e.UserState as Message);
                }
                else if (e.ProgressPercentage == 2)
                {
                    OnUserRoomsChanged(e.UserState as List<ChatRoom>, ChatRoom.RoomChangeType.New);
                    _usersChatRoom.AddRange(e.UserState as List<ChatRoom>);
                }
            };

            worker.RunWorkerAsync();
        }

        public bool IsPrivateChatRoomWithUser(User User)
        {
            List<int> users = new List<int>();
            users.Add(User.userid);
            users.Add(this.userid);

            bool returnValue = false;
            _usersChatRoom.ForEach(p =>
            {
                if (returnValue == false)
                {
                    var listOfUsersId = GetListOfIdFromText(p.UsersString);
                    if (listOfUsersId.Except(users).Count() == 0)
                    {
                        returnValue = true;
                    }
                }
            });

            return returnValue;
        }
        public async Task<List<ChatRoom>> GetUserChatRooms(bool NewList = true)
        {
            bool downloadList = false;

            if (NewList)
            {
                downloadList = true;
            }
            else
            {
                downloadList = _usersChatRoom == null ? true : false;
            }

            return downloadList ? _usersChatRoom = (await _GetMultiListFromIdies<ChatRoom>(null, this.userid, true)) ?? new List<ChatRoom>() : _usersChatRoom;
        }
        public async Task<bool> UpdateUserInformation(List<ChatRoom> ChatRooms = null)
        {
            return await _UpdateUserInformation(this.userid, ChatRooms);
        }
        public async Task<bool> RemoveThisUserFromChatRoom(ChatRoom Room)
        {
            var res = await _RemoveUserFromChatRoom(this, Room);

            if (res)
            {
                this.messages = this.messages.Replace($"{Room.Id}#", "");
                var itemToDelete = _usersChatRoom.FirstOrDefault(p => p.Id == Room.Id);
                if(itemToDelete != null)
                {
                    _usersChatRoom.Remove(itemToDelete);
                }
            }

            return res;
        }
    }
}
