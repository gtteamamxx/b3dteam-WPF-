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
using b3dteam_app.Model;
using System.Collections.ObjectModel;

namespace b3dteam_app.View.ChatUtilities
{
    /// <summary>
    /// Interaction logic for OnlineUsers.xaml
    /// </summary>
    public partial class OnlineUsers : Window
    {
        private ObservableCollection<ChatUser> _OffineUsersList;
        private ObservableCollection<ChatUser> _OnlineUsersList;

        private ObservableCollection<ChatUser> _UserList;
        public OnlineUsers()
        {
            InitializeComponent();

            _OffineUsersList = new ObservableCollection<ChatUser>();
            _OnlineUsersList = new ObservableCollection<ChatUser>();
            _UserList = new ObservableCollection<ChatUser>();

            this.Loaded += OnlineUsers_Loaded;

            this.SizeChanged += (s, e) =>
            {
                listview_Users.Height = e.NewSize.Height - 60.0;
            };
        }

        private void OnlineUsers_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshTable();
        }

        private void RefreshTable(string name = "")
        {
            textblock_Info.Visibility = Visibility.Visible;
            _OffineUsersList.Clear();
            _OnlineUsersList.Clear();
            _UserList.Clear();
            listview_Users.Items.Clear();

            var users = Chat.GetBall3DServer().Users;

            if (users.Count() == 0)
            {
                users = Chat._DiscordClient.GetServer(Chat.GetBall3DServer().Id).Users;
            }

            if (users.Count() == 0)
            {
                textblock_Info.Text = "Error occured, open window again.";
                return;
            }
            foreach (var user in users)
            {
                if(name.Length > 0 && !user.Name.ToLower().Contains(name))
                {
                    continue;
                }

                var tempUser = Chat.GetBall3DServer().GetUser(user.Id);

                var u = new ChatUser()
                {
                    userName = tempUser.Name,
                    statusImage = new BitmapImage(new Uri(tempUser.Status == Discord.UserStatus.Online ? "http://vignette1.wikia.nocookie.net/pocketplanes/images/5/55/Online_status.png/revision/latest?cb=20120721162015"
                    : "http://yourlivestore.com/templates/tmpl_livestore/images/icons/offline.png"))
                };


                if (tempUser.Status == Discord.UserStatus.Online || tempUser.Status == Discord.UserStatus.DoNotDisturb)
                {
                    _OnlineUsersList.Add(u);
                }
                else
                {
                    _OffineUsersList.Add(u);
                }
            }

            foreach (var item in _OnlineUsersList)
            {
                _UserList.Add(item);
            }
            foreach (var item in _OffineUsersList)
            {
                _UserList.Add(item);
            }

            foreach (var user in _UserList)
            {
                var stackpanel = new StackPanel() { Orientation = Orientation.Horizontal } ;

                stackpanel.Children.Add(new TextBlock() { Text = user.userName });
                stackpanel.Children.Add(new Image()
                {
                    Margin = new Thickness(10, 0, 0, 0),
                    Source = user.statusImage,
                    Width = 20,
                    Height = 20
                });

                listview_Users.Items.Add(stackpanel);
            }

            textblock_Info.Visibility = Visibility.Collapsed;
        }

        private void textbox_FindUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(textbox_FindUser.Text.Contains("Find user...") || (textblock_Info != null && textblock_Info.Visibility == Visibility.Visible))
            {
                return;
            }

            RefreshTable(textbox_FindUser.Text.ToLower());
        }
    }
}
