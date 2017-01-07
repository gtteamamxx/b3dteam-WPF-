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

        private async void button_Login_Click(object sender, RoutedEventArgs e)
        {
            var loginSuccesful = await Model.SQLManager.LoginUser(textbox_Login.Text, textbox_Password.Password);

            switch(loginSuccesful)
            {
                case Model.SQLManager.LoginAccountStatus.Bad_Authorization:
                    MessageBox.Show("Login or password are incorrect", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;

                case Model.SQLManager.LoginAccountStatus.Succesful:
                    LoginWindow.gui.Close();
                    break;

                case Model.SQLManager.LoginAccountStatus.Failed:
                    MessageBox.Show("There was problem with login. Try again later", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case Model.SQLManager.LoginAccountStatus.Account_Not_Activated:
                    MessageBox.Show("This account is not activated yet." + Environment.NewLine + "Write to grs4_98@o2.pl, or on Gadu-Gadu 38862128", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                    break;
            }
        }
    }
}
