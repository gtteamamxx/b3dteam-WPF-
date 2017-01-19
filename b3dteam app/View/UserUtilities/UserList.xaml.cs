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
    /// Interaction logic for UserList.xaml
    /// </summary>
    public partial class UserList : Window
    {
        private List<helper.User> ListOfUsers;

        public UserList(bool defualtLoad = true)
        {
            InitializeComponent();

            ListOfUsers = new List<helper.User>();

            if (defualtLoad)
            {
                this.Loaded += UserList_Loaded;
            }
        }            

        private async void UserList_Loaded(object sender, RoutedEventArgs e)
        {
            textblock_Info.Visibility = Visibility.Visible;

            ListOfUsers = await helper.SQLManager.GetUsers();

            if (ListOfUsers == null || ListOfUsers.Count == 0)
            {
                textblock_Info.Text = "Error occured. Reopen this window again, or try again later.";
                return;
            }

            SetDataToListView(ListOfUsers);

            textblock_Info.Visibility = Visibility.Collapsed;
        }

        private void SetDataToListView(IEnumerable<helper.User> usersList)
        {
            listview_Users.Items.Clear();

            foreach (var user in usersList)
            {
                var stackpanel = new StackPanel() { Orientation = Orientation.Horizontal };

                stackpanel.Children.Add(new TextBlock() { Text = user.login });

                var image = new BitmapImage(new Uri(user.GetuserStatus() == helper.SQLManager.Ball3D_Status.Status_Online ? "http://vignette1.wikia.nocookie.net/pocketplanes/images/5/55/Online_status.png/revision/latest?cb=20120721162015"
                    : "http://yourlivestore.com/templates/tmpl_livestore/images/icons/offline.png"));

                stackpanel.Children.Add(new Image()
                {
                    Margin = new Thickness(10, 0, 0, 0),
                    Source = image,
                    Width = 20,
                    Height = 20
                });

                listview_Users.Items.Add(stackpanel);
            }
        }

        private void textbox_FindUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textbox_FindUser.Text.Contains("Find user...") || (textblock_Info != null && textblock_Info.Visibility == Visibility.Visible))
            {
                return;
            }

            SetDataToListView(ListOfUsers.Where(p => p.login.ToLower().StartsWith(textbox_FindUser.Text.ToLower())));
        }

        private void listview_Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            grid_UserInfo.Visibility = Visibility.Visible;
            var userName = ((listview_Users.SelectedItem as StackPanel).Children[0] as TextBlock).Text;
            ShowUserInfo(userName);
        }

        public async void ShowUserInfo(string userName, bool hideFindUsers = false)
        {
            helper.User user = null;

            if(hideFindUsers)
            {
                grid_FindUser.Visibility = Visibility.Collapsed;
                user = await helper.SQLManager.GetUser(null, userName);
            }
            else
            {
                user = ListOfUsers.First(p => p.login == userName);
            }

            textbox_UserInfoAccountType.Text = user.GetUserAccountTypeName();

            var userLastActivityDateTime = user.GetDateTimeFromLatActivity();
            textbox_UserInfoLastActivity.Text = $"{userLastActivityDateTime.ToShortDateString()} {userLastActivityDateTime.ToShortTimeString()}";
            textbox_UserInfoLogin.Text = user.login;
            image_UserInfoLatActivity.Source = new BitmapImage(new Uri(user.GetuserStatus() == helper.SQLManager.Ball3D_Status.Status_Online ? "http://vignette1.wikia.nocookie.net/pocketplanes/images/5/55/Online_status.png/revision/latest?cb=20120721162015"
                    : "http://yourlivestore.com/templates/tmpl_livestore/images/icons/offline.png"));

            //if firend

            textbox_UserInfoIsFriend.Text = "User is your friend";
        }
    }
}
