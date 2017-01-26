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
        private static readonly double CHECK_MESSAGE_TIME = 10.0;
        private static SqlConnection _Connection;

        public delegate void UserRoomsChanged(List<ChatRoom> ChatRoom, ChatRoom.RoomChangeType ChangeType);
        public delegate void MessageReceived(Message message);

        public event UserRoomsChanged OnUserRoomsChanged;
        public event MessageReceived OnMessageReceived;

        public int userid { get; protected internal set; }
        public string login { get; protected internal set; }
        public string password { get; protected internal set; }
        public string email { get; protected internal set; }
        public int usertype { get; protected internal set; }
        public int lastactivity { get; protected internal set; }
        public int regtime { get; protected internal set; }
        public string messages { get; protected internal set; }
        public string userteams { get; protected internal set; }

        private List<ChatRoom> _usersChatRoom { get; set; }
        internal protected int _LastMessageId { get; set; }

        public User()
        {
            _usersChatRoom = new List<ChatRoom>();
        }

        ~User()
        {
            if (_Connection != null)
            {
                _Connection.Close();
            }
        }
        
        public void StartListeningChanges()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;

            worker.DoWork += (s, e) =>
            {
                SqlCommand command = new SqlCommand();
                while(true)
                {
                    Task.Delay(TimeSpan.FromSeconds(CHECK_MESSAGE_TIME)).GetAwaiter().GetResult();

                    _Connection = SQLC.Connection.GetConnection();
                    command = new SqlCommand() { Connection = _Connection };
                    command.CommandType = System.Data.CommandType.Text;

                    string query = $"SELECT messages FROM USERS WHERE userid = {this.userid};";

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

                        while (reader.Read())
                        {
                            msg = reader.GetString(0);
                            break;
                        }

                        if (msg.Length > 0 && !string.Equals(this.messages, msg))
                        {
                            var lastRooms = GetListOfIdFromText(this.messages);
                            var newRooms = GetListOfIdFromText(msg);

                            this.messages = msg;

                            foreach (var last in lastRooms)
                                if (!newRooms.Contains(last)) listDeletedRooms.Add(new ChatRoom() { Id = last });

                            if (listDeletedRooms.Count > 0)
                            {
                                worker.ReportProgress(0, listDeletedRooms);
                            }

                            tempListWithIDOfNewRooms = newRooms.Where(p => !lastRooms.Contains(p)).ToList();
                        }

                        if(reader.NextResult())
                        {
                            while(reader.Read())
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
                                    owner = _GetUser(reader.GetInt32(3)).Result,
                                    timestamp = reader.GetInt32(4)
                                };

                                worker.ReportProgress(1, message);
                            }
                        }

                        if(listDeletedRooms.Count > 0 )
                        {
                            worker.ReportProgress(0, listDeletedRooms);
                        }

                        if(tempListWithIDOfNewRooms.Count > 0)
                        {
                            listNewRooms = _GetMultiListFromIdies<ChatRoom>(tempListWithIDOfNewRooms).Result;
                            worker.ReportProgress(2, listNewRooms);
                        }
                    }
                }
            };

            worker.ProgressChanged += (s, e) =>
            {
                if(e.ProgressPercentage == 0)
                {
                    OnUserRoomsChanged(e.UserState as List<ChatRoom>, ChatRoom.RoomChangeType.Deleted);
                }
                else if (e.ProgressPercentage == 1)
                {
                    OnMessageReceived(e.UserState as Message);
                }
                else if(e.ProgressPercentage == 2)
                {
                    OnUserRoomsChanged(e.UserState as List<ChatRoom>, ChatRoom.RoomChangeType.New);
                }
            };

            worker.RunWorkerAsync();
        }

        public async Task<bool> IsChatRoomWithUser(User User)
        {
            return (await GetUserChatRooms(false)).Any(p => p.Users.Any(d => d.userid == User.userid));
        }
        public async Task<List<ChatRoom>> GetUserChatRooms(bool NewList = true)
        {
            bool downloadList = false;

            if(NewList)
            {
                downloadList = true;
            }
            else
            {
                downloadList = _usersChatRoom == null ? true : false;
            }

            return downloadList ? _usersChatRoom = await _GetMultiListFromIdies<ChatRoom>(null, this.userid, true)??new List<ChatRoom>() : _usersChatRoom;
        }

        public async Task<bool> UpdateUserInformation(List<ChatRoom> ChatRooms = null)
        {
            return await _UpdateUserInformation(this.userid, ChatRooms);
        }

        private void UpdateClientChatChannels()
        {

        }
    }
}
