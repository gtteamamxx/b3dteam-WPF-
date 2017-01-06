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

namespace b3dteam.View
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void button_Login_Click(object sender, RoutedEventArgs e)
        {
            frame_Center.NavigationService.Navigate(new LoginWindow_Login());
        }

        private void button_Register_Click(object sender, RoutedEventArgs e)
        {
            frame_Center.NavigationService.Navigate(new LoginWindow_Register());
        }
    }
}
