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
        public static LoginWindow gui;

        public LoginWindow()
        {
            InitializeComponent();
            gui = this;
        }

        private void HideButtons()
        {
            button_Login.Visibility = Visibility.Collapsed;
            button_Register.Visibility = Visibility.Collapsed;
        }

        public void ShowMainView()
        {
            button_Login.Visibility = Visibility.Visible;
            button_Register.Visibility = Visibility.Visible;

            frame_Center.Navigate(null);
        }
        private void button_Login_Click(object sender, RoutedEventArgs e)
        {
            HideButtons();
            frame_Center.NavigationService.Navigate(new LoginWindow_Login(), this);
        }

        private void button_Register_Click(object sender, RoutedEventArgs e)
        {
            HideButtons();
            frame_Center.NavigationService.Navigate(new LoginWindow_Register());
        }
    }
}
