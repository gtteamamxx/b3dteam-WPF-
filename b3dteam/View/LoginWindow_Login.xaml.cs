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

namespace b3dteam.View
{
    /// <summary>
    /// Interaction logic for LoginWindow_Login.xaml
    /// </summary>
    public partial class LoginWindow_Login : UserControl
    {
        public LoginWindow_Login()
        {
            InitializeComponent();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow.gui.ShowMainView();
        }

        private void button_Login_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
