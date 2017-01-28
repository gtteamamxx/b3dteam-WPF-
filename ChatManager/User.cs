using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        internal protected List<ChatRoom> _UsersChatRoom { get; set; }
        internal protected int _LastMessageId { get; set; }

        internal protected List<Message> _ListOfsendedMessagesButNotFetched;

        public User()
        {
            _UsersChatRoom = new List<ChatRoom>();
            _ListOfsendedMessagesButNotFetched = new List<Message>();
    }

    #region Listening Changes
    public void StartListeningChanges()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;

            worker.DoWork += (s, e) =>
            {
                MySqlCommand command = new MySqlCommand();
                command.CommandType = System.Data.CommandType.Text;
                string query = string.Empty;

                while (true)
                {
                    Task.Delay(TimeSpan.FromSeconds(CHECK_MESSAGE_TIME)).GetAwaiter().GetResult();

                    using (var _Connection = SQLC.Connection.GetConnection())
                    {
                        command.Connection = _Connection;

                        query = $"SELECT messages FROM USERS WHERE userid = {this.userid};";

                        if (this.messages.Length > 0)
                        {
                            query += $" SELECT * FROM MESSAGES WHERE message_id > {this._LastMessageId}";

                            if (this._ListOfsendedMessagesButNotFetched.Count() > 0)
                            {
                                foreach(Message msg in this._ListOfsendedMessagesButNotFetched)
                                {
                                    query += $" AND message_id <> {msg.message_id}";
                                }
                            }

                            query += $" AND chat_room_id IN ({GetIDSequence(GetListOfIdFromText(this.messages))});";
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
                                    this._UsersChatRoom.Remove(this._UsersChatRoom.First(g => g.Id == p));
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
                                        this._LastMessageId = reader.GetInt32(0);
                                    }

                                    var message = new Message()
                                    {
                                        message_id = reader.GetInt32(0),
                                        chat_room_id = reader.GetInt32(1),
                                        message = reader.GetString(2),
                                        ownerId = reader.GetInt32(3),
                                        ownerName = reader.GetString(4),
                                        timestamp = reader.GetInt32(5)
                                    };

                                    worker.ReportProgress(1, message);
                                }

                                if (this._ListOfsendedMessagesButNotFetched.Count > 0)
                                {
                                    int maxValueOfIdInTempMessages = this._ListOfsendedMessagesButNotFetched.Max(p => p.message_id);

                                    this._LastMessageId = maxValueOfIdInTempMessages > this._LastMessageId ? maxValueOfIdInTempMessages : this._LastMessageId;

                                    this._ListOfsendedMessagesButNotFetched.Clear();
                                }
                            }

                            if (listDeletedRooms.Count > 0)
                            {
                                worker.ReportProgress(0, listDeletedRooms);
                            }

                            if (tempListWithIDOfNewRooms.Count > 0)
                            {
                                listNewRooms = _GetMultiListFromIdies<ChatRoom>(tempListWithIDOfNewRooms).Result.OrderByDescending(p => p.Id).ToList();
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
                    _UsersChatRoom.AddRange(e.UserState as List<ChatRoom>);
                }
            };

            worker.RunWorkerAsync();
        }

        #endregion

        #region Is This user in privaste room with otheru ser
        public bool IsPrivateChatRoomWithUser(User User)
        {
            List<int> users = new List<int>();
            users.Add(User.userid);
            users.Add(this.userid);

            bool returnValue = false;
            _UsersChatRoom.ForEach(p =>
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

        #endregion

        #region Get User Chat Rooms
        public async Task<List<ChatRoom>> GetUserChatRooms(bool NewList = true)
        {
            bool downloadList = false;

            if (NewList)
            {
                downloadList = true;
            }
            else
            {
                downloadList = _UsersChatRoom == null ? true : false;
            }

            return downloadList ? _UsersChatRoom = (await _GetMultiListFromIdies<ChatRoom>(null, this.userid, true)) ?? new List<ChatRoom>() : _UsersChatRoom;
        }

        #endregion

        public async Task<bool> UpdateUserInformation(List<ChatRoom> ChatRooms = null)
        {
            return await _UpdateUserInformation(this.userid, ChatRooms);
        }

        #region Remove This user From Chat Room
        public async Task<bool> RemoveThisUserFromChatRoom(ChatRoom Room)
        {
            var res = await _RemoveUserFromChatRoom(this, Room);

            if (res)
            {
                this.messages = this.messages.Replace($"{Room.Id}#", "");
                var itemToDelete = _UsersChatRoom.FirstOrDefault(p => p.Id == Room.Id);
                if(itemToDelete != null)
                {
                    _UsersChatRoom.Remove(itemToDelete);
                }
            }

            return res;
        }
        #endregion

        public DateTime GetDateTimeFromLastActivity() => _GetDateTimeFromTimeStamp(this.lastactivity);
        public DateTime GetDateTimeFromRegisterTime() => _GetDateTimeFromTimeStamp(this.regtime);
    }
}
