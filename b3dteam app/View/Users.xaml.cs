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
        public Users()
        {
            InitializeComponent();
        }

        private void button_AddContact_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_RemoveContact_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_UserList_Click(object sender, RoutedEventArgs e)
        {
            var userListWindow = new UserUtilities.UserList();
            userListWindow.Show();
        }
    }
}
