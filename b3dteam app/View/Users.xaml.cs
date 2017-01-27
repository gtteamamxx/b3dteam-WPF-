using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace b3dteam_app.View
{
    /// <summary>
    /// Interaction logic for Users.xaml
    /// </summary>
    public partial class Users : UserControl
    {
        public static ChatManager.Chat ChatEngine;
        public static ChatManager.User ClientUser;

        public Users()
        {
            InitializeComponent();
            ChatEngine = new ChatManager.Chat();
            ClientUser = new ChatManager.User();

            GetUserAndRooms();
        }


        private async void GetUserAndRooms()
        {
            listview_Contact.IsEnabled = false;

            textblock_Update.Text = "Getting user...";
            ClientUser = await ChatEngine.GetUser(helper.User.ClientUser.userid, GetLastMessage: true);

            if (ClientUser == null)
            {
                MessageBox.Show("There was problem wtih this account. Try again later.");
                MainWindow.gui.Close();
                return;
            }
            else
            {
                ClientUser.OnMessageReceived += ClientUser_OnMessageReceived;
                ClientUser.OnUserRoomsChanged += ClientUser_OnUserRoomsChanged; ; ;

                textblock_Update.Text = "Getting talks...";
                (await ClientUser.GetUserChatRooms()).ForEach(p => AddChatRoom(p));

                ClientUser.StartListeningChanges();
                textblock_Update.Text = $"Your talks: {ClientUser.GetUserChatRooms(false).Result.Count}";
                listview_Contact.IsEnabled = true;
            }
        }

        private void ClientUser_OnUserRoomsChanged(List<ChatManager.ChatRoom> ChatRoom, ChatManager.ChatRoom.RoomChangeType ChangeType)
        {
            if(ChangeType == ChatManager.ChatRoom.RoomChangeType.New)
            {
                ChatRoom.ForEach(p => AddChatRoom(p));
            }
            else if(ChangeType == ChatManager.ChatRoom.RoomChangeType.Deleted)
            {
                ChatRoom.ForEach(async p => await RemoveChatRoom(p));
            }

            textblock_Update.Text = $"Your talks: {ClientUser.GetUserChatRooms(false).Result.Count}";
        }

        private void ClientUser_OnMessageReceived(ChatManager.Message message)
        {
            throw new NotImplementedException();
        }

        private void button_AddContact_Click(object sender, RoutedEventArgs e)
        {
            new UserUtilities.AddContact(this).ShowDialog();
        }

        private void button_RemoveContact_Click(object sender, RoutedEventArgs e)
        {
            new UserUtilities.RemoveContact(this).ShowDialog();
        }

        private void button_UserList_Click(object sender, RoutedEventArgs e)
        {
            var userListWindow = new UserUtilities.UserList();
            userListWindow.Show();
        }

        private void textbox_MessagePrivate_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void listview_Contact_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public async Task<bool> AddContactToList(ChatManager.User userToAdd = null, ChatManager.ChatRoom ChatRoom = null)
        {
            if(userToAdd != null)
            {
                var users = new List<ChatManager.User>();
                users.Add(userToAdd);
                var channel = await ChatEngine.CreateChatRoom(userToAdd.login, users, ClientUser);

                if (channel == null)
                {
                    return false;
                }
            }

            if(ChatRoom != null)
            {
                AddChatRoom(ChatRoom);
            }

            return true;
        }

        public async Task<bool> RemoveChatRoom(ChatManager.ChatRoom chatRoom, bool UserLeave = false)
        {
            if (chatRoom != null)
            {
                if (UserLeave)
                {
                    var res = await ClientUser.RemoveThisUserFromChatRoom(chatRoom);

                    if (res == false)
                    {
                        return false;
                    }
                }

                var itemToRemove = Chat.FindVisualChildren<Grid>(this).First(p => p.Name == "chatroom" && int.Parse(((TextBlock)p.Children[0]).Text.Replace("#", "")) == chatRoom.Id);
                listview_Contact.Items.Remove(itemToRemove);
            }
            return true;
        }

        private void AddChatRoom(ChatManager.ChatRoom chatRoom)
        {
            var grid = new Grid();

            grid.Name = "chatroom";

            grid.Children.Add(new TextBlock()
            {
                Foreground = new SolidColorBrush(Colors.Gray),
                Text = $"#{chatRoom.Id}"
            });

            grid.Children.Add(new TextBlock()
            {
                Text = chatRoom.Name,
                Margin = new Thickness(40, 0, 0, 0)
            });

            listview_Contact.Items.Add(grid);
        }
    }
}
