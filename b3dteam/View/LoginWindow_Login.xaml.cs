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

            checkbox_AutoLogin.IsChecked = Properties.Settings.Default.autologin;
            checkbox_Rememberme.IsChecked = Properties.Settings.Default.rememberme;

            if (checkbox_AutoLogin.IsChecked == true)
            {
                checkbox_Rememberme.IsChecked = true;
                checkbox_Rememberme.IsEnabled = false;
            }

            if (checkbox_Rememberme.IsChecked == true)
            {
                textbox_Login.Text = Properties.Settings.Default.login;
                textbox_Password.Password = Properties.Settings.Default.password;
            }

            /////////////////////////////////////////
            checkbox_AutoLogin.Unchecked += (s, e) =>
            {
                Properties.Settings.Default.autologin = false;
                Properties.Settings.Default.Save();
                checkbox_Rememberme.IsEnabled = true;
            };

            checkbox_AutoLogin.Checked += (s, e) =>
            {
                checkbox_Rememberme.IsChecked = true;
                checkbox_Rememberme.IsEnabled = false;

                Properties.Settings.Default.autologin = true;
                Properties.Settings.Default.Save();
            };

            //////////////////////////////////////////
            checkbox_Rememberme.Unchecked += (s, e) =>
            {
                Properties.Settings.Default.rememberme = false;
                Model.Extension.ResetUser(MainWindow.ClientUser);
            };
            checkbox_Rememberme.Checked += (s, e) =>
            {
                Properties.Settings.Default.rememberme = true;
                Properties.Settings.Default.Save();
            };

            FocusManager.SetFocusedElement(this, textbox_Login);
            textbox_Login.SelectAll();

            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter || e.Key == Key.Space)
                {
                    button_Login_Click(null, null);
                }
            };
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow.gui.ShowMainView();
        }

        private async void button_Login_Click(object sender, RoutedEventArgs e)
        {
            var pass = Properties.Settings.Default.rememberme == true && textbox_Password.Password.Length > 16 ? Properties.Settings.Default.password : helper.Cryptography.Sha256(textbox_Password.Password);

            var loginTuple = await helper.SQLManager.LoginUser(textbox_Login.Text, pass);

            helper.SQLManager.LoginAccountStatus loginAccountStatus = loginTuple.Item1;

            switch (loginAccountStatus)
            {
                case helper.SQLManager.LoginAccountStatus.Bad_Authorization:
                    MessageBox.Show("Login or password are incorrect", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;

                case helper.SQLManager.LoginAccountStatus.Succesful:
                    var user = loginTuple.Item2;
                    Model.Extension.SaveUser(user);
                    MainWindow.ClientUser = user;
                    LoginWindow.gui.Close();
                    break;

                case helper.SQLManager.LoginAccountStatus.Failed:
                    MessageBox.Show("There was problem with login. Try again later", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case helper.SQLManager.LoginAccountStatus.Account_Not_Activated:
                    MessageBox.Show("This account is not activated yet.{Environment.NewLine}Write to grs4_98@o2.pl, or on Gadu-Gadu 38862128", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                    break;
            }
        }
    }
}
