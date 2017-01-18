using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace b3dteam_app.View.ChatUtilities
{
    /// <summary>
    /// Interaction logic for PrivateMessage.xaml
    /// </summary>
    public partial class PrivateMessage : Window
    {
        public static ObservableCollection<Tuple<Model.Server, PrivateMessageUserControl>> ListOfPrivateChannels = new ObservableCollection<Tuple<Model.Server, PrivateMessageUserControl>>();
        public static PrivateMessage gui;
        public static System.Windows.Forms.RichTextBox GetRichTextBox => gui.textbox_Chat.Child as System.Windows.Forms.RichTextBox;

        public static void AddPrivateChannelIfNeccessary(Model.Server server)
        {
            if(ListOfPrivateChannels == null || server == null)
            {
                return;
            }

            var result = ListOfPrivateChannels.FirstOrDefault(p => p.Item1.Id == server.Id);

            if (result == null)
            {
                ListOfPrivateChannels.Add(new Tuple<Model.Server, PrivateMessageUserControl>(server, null));
            }
            else
            {
                ListOfPrivateChannels[ListOfPrivateChannels.IndexOf(result)] = new Tuple<Model.Server, PrivateMessageUserControl>(server, result.Item2);
                result.Item2.ChangeData(server);
            }
        }

        public static void SetUserControlToChannel(PrivateMessageUserControl user, Model.Server server)
        {
            var result = ListOfPrivateChannels.FirstOrDefault(p => p.Item1.Id == server.Id);

            if(result == null)
            {
                return;
            }
            else
            {
                ListOfPrivateChannels[ListOfPrivateChannels.IndexOf(result)] = new Tuple<Model.Server, PrivateMessageUserControl>(server, user);
            }
        }
        public PrivateMessage()
        {
            InitializeComponent();
            gui = this;

            this.Loaded += PrivateMessage_Loaded;
            GetRichTextBox.MouseClick += Chat.GetRichTextBox_MouseClick;
            listview_PrivateChannels.SelectionChanged += Listview_PrivateChannels_SelectionChanged;

        }

        private async void Listview_PrivateChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = listview_PrivateChannels.SelectedItem as Model.Server;
            int numOfUnreadedMessages = -1;

            try
            {
                numOfUnreadedMessages = int.Parse(selectedItem.UnreadedMessages);
            }
            catch { }

            var channelId = selectedItem.Id;

            var messagesFromChannel = await Chat.gui.DownloadLastMessages(channelId);

            messagesFromChannel.Reverse();
            messagesFromChannel.ToList().ForEach(p => Chat.AddMessage(GetRichTextBox, p));

            if (numOfUnreadedMessages != -1)
            {
                selectedItem.UnreadedMessages = "";
                var result = ListOfPrivateChannels.FirstOrDefault(p => p.Item1.Id == selectedItem.Id);
                ListOfPrivateChannels[ListOfPrivateChannels.IndexOf(result)] = new Tuple<Model.Server, PrivateMessageUserControl>(selectedItem, result.Item2);
                result.Item2.ChangeData(selectedItem);
                Chat.gui.SetUnreadedMessageStatus(selectedItem.Name, false, true, numOfUnreadedMessages);
            }
        }

        private void PrivateMessage_Loaded(object sender, RoutedEventArgs e)
        {
            listview_PrivateChannels.Items.Clear();

            ListOfPrivateChannels.ToList().ForEach(p =>
            {
                listview_PrivateChannels.Items.Add(p.Item1);
            });
        }

        private async void textbox_MessagePrivate_KeyUp(object sender, KeyEventArgs e)
        {
            await Chat.textbox_Message_KeyUpped(sender, e);
        }

        private async void button_SendFile_Click(object sender, RoutedEventArgs e)
        {
            await Chat.button_SendFile_Clicked(sender, e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            gui = null;
            base.OnClosing(e);
        }

        private void button_NewMessage_Click(object sender, RoutedEventArgs e)
        {
            var onlineUsersWindow = new OnlineUsers();
            onlineUsersWindow.ShowDialog();
        }

    }
}
