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

        public System.Windows.Forms.RichTextBox GetRichTextBox => (System.Windows.Forms.RichTextBox)textbox_Chat.Child;
        private ChatManager.ChatRoom GetSelectedChatRoom() => ClientUser.GetUserChatRooms(false).Result.FirstOrDefault(p => p.Id == int.Parse(((listview_Contact.SelectedItem as Grid).Children[0] as TextBlock).Text.Replace("#", "")));
        

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
            if(GetSelectedChatRoom().Id == message.chat_room_id)
            {
                AddMessage(GetRichTextBox, message);
            }
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
            
            new UserUtilities.UserList().Show();
        }

        private static bool _IsSendingMessage = false;

        private async void textbox_MessagePrivate_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift && !_IsSendingMessage)
            {
                var messageText = textbox_MessagePrivate.Text.Trim();

                if (messageText.Length == 0)
                {
                    return;
                }
                else if(messageText.Length > 240)
                {
                    MessageBox.Show("Max lenght of one message is 240 chars.", "Message too long", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _IsSendingMessage = true;
                textbox_MessagePrivate.IsReadOnly = true;

                textbox_MessagePrivate.Text = "Sending... : " + messageText;
                await Task.Delay(50);

                var message = await GetSelectedChatRoom().SendMessage(ClientUser, messageText);

                textbox_MessagePrivate.Text = "";

                textbox_MessagePrivate.IsReadOnly = false;
                _IsSendingMessage = false;

                if (message != null)
                {
                    AddMessage(GetRichTextBox, message);
                }
                else
                {
                    throw new NotImplementedException();
                }

                e.Handled = true;
            }
        }

        #region Adding message to TextBox
        private DateTime _LastMessageTime = new DateTime();
        public void AddMessage(System.Windows.Forms.RichTextBox richTextBox, ChatManager.Message message)
        {
            var author = string.IsNullOrEmpty(message.ownerName)? "Unknown user" : message.ownerName;

            CheckLastMessageTimeAndAddDateLineIfNeeded(richTextBox, message);

            richTextBox.AppendText($"{message.GetDateTimeFromTimeStamp().ToShortTimeString()}");
            AppendTextToRichTextBoxForms(richTextBox, $" {author}:", Chat.gui.GetColorOfNick(author), false);
            richTextBox.AppendText($" {message.message}\n");
            richTextBox.SelectionStart = richTextBox.Text.Length;
            richTextBox.ScrollToCaret();
        }

        private void CheckLastMessageTimeAndAddDateLineIfNeeded(System.Windows.Forms.RichTextBox richTextBox, ChatManager.Message message)
        {
            DateTime messageDateTime = message.GetDateTimeFromTimeStamp();
            if (_LastMessageTime.Day != messageDateTime.Day)
            {
                AppendTextToRichTextBoxForms(richTextBox, $"=================={messageDateTime.ToShortDateString()}==================", System.Drawing.Color.Red, true);
                _LastMessageTime = messageDateTime;
            }
        }

        //from so
        public void AppendTextToRichTextBoxForms(System.Windows.Forms.RichTextBox richTextBox, string text, System.Drawing.Color color, bool AddNewLine = false)
        {
            if (AddNewLine)
            {
                text += Environment.NewLine;
            }

            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;

            richTextBox.SelectionColor = color;
            richTextBox.AppendText(text);
            richTextBox.SelectionColor = richTextBox.ForeColor;
        }

        #endregion

        private async void listview_Contact_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetRichTextBox.Clear();

            if (listview_Contact.SelectedItem == null)
            {
                return;
            }
            AppendTextToRichTextBoxForms(GetRichTextBox, "===============DOWNLOADING MESSAGES.....===============\r\n", System.Drawing.Color.Red);
            var messages = await GetSelectedChatRoom().GetMessages();

            if (messages == null)
            {
                AppendTextToRichTextBoxForms(GetRichTextBox, "===============ERROR WHILE DOWNLOADING MESSAGES===============", System.Drawing.Color.Red);
                return;
            }

            if (messages.Count() > 0)
            {
                GetRichTextBox.Clear();
                messages.ForEach(p => AddMessage(GetRichTextBox, p));
            }
            else
            {
                AppendTextToRichTextBoxForms(GetRichTextBox, "===============NO MESSAGES IN THIS TALK===============\r\n", System.Drawing.Color.Red);
            }
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

                var selectedChatRoomInListView = GetSelectedChatRoom();

                if (selectedChatRoomInListView != null && chatRoom.Id == selectedChatRoomInListView.Id)
                {
                    GetRichTextBox.Clear();
                }

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
