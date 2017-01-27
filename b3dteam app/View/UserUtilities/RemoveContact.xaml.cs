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
using System.Windows.Shapes;

namespace b3dteam_app.View.UserUtilities
{
    /// <summary>
    /// Interaction logic for RemoveContact.xaml
    /// </summary>
    public partial class RemoveContact : Window
    {
        private Users usersWindow;
        public RemoveContact(Users users)
        {
            InitializeComponent();
            usersWindow = users;

            this.Loaded += RemoveContact_Loaded;
        }

        private async void RemoveContact_Loaded(object sender, RoutedEventArgs e)
        {
            button_Leave.IsEnabled = false;
            var user = Users.ClientUser;
            var channels = await user.GetUserChatRooms(false);

            channels.ForEach(p => AdddRoomToList(p));
            button_Leave.IsEnabled = true;
        }
        private void AdddRoomToList(ChatManager.ChatRoom chatRoom)
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

            listview_Users.Items.Add(grid);
        }
        private async void button_Leave_Click(object sender, RoutedEventArgs e)
        {
            button_Leave.IsEnabled = false;

            var selectedRoom = Users.ClientUser.GetUserChatRooms(false).Result.FirstOrDefault(p => p.Id == int.Parse(((listview_Users.SelectedItem as Grid).Children[0] as TextBlock).Text.Replace("#", "")));
            if(selectedRoom == null)
            {
                return;
            }
            if(await usersWindow.RemoveChatRoom(selectedRoom))
            {
                MessageBox.Show("Talk has been removed");
                listview_Users.Items.Remove(listview_Users.SelectedItem);
            }
            else
            {
                MessageBox.Show("There was problem with removing a talk.");
            }

            button_Leave.IsEnabled = true;
        }
    }
}
