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

namespace b3dteam_app.View.Options.Options
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();

            FillLoginInfo();
            SetCheckedRadioButtons();


            radio_AutoLogin_Enabled.Checked += (s, e) =>
            {
                radio_RememberLoginInf_Disabled.IsEnabled = false;
                radio_RememberLoginInf_Enabled.IsChecked = true;
                radio_RememberLoginInf_Enabled.IsEnabled = false;
            };
            radio_AutoLogin_Disabled.Checked += (s, e) =>
            {
                radio_RememberLoginInf_Disabled.IsEnabled = true;
                radio_RememberLoginInf_Enabled.IsEnabled = true;
            };
        }

        private void SetCheckedRadioButtons()
        {
            if(helper.User.ClientUser.autologin == 1)
            {
                radio_AutoLogin_Enabled.IsChecked = true;
            }
            else
            {
                radio_AutoLogin_Disabled.IsChecked = true;
            }

            if(helper.User.ClientUser.rememberme == 1)
            {
                radio_RememberLoginInf_Enabled.IsChecked = true;
            }
            else
            {
                radio_RememberLoginInf_Disabled.IsChecked = true;
            }

            if(radio_AutoLogin_Enabled.IsChecked == true)
            {
                radio_RememberLoginInf_Disabled.IsEnabled = false;
                radio_RememberLoginInf_Enabled.IsEnabled = false;
            }
        }

        private void FillLoginInfo()
        {
            textbox_Login.Text = helper.User.ClientUser.login;
            textbox_Password.Password = helper.User.ClientUser.password;
        }
        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveRememberLoginStatus();
            SaveAutoLoginStatus();
            helper.User.Save();
            MessageBox.Show("Save completed", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow.gui.Close();
        }

        private void SaveAutoLoginStatus()
        {
            helper.User.ClientUser.autologin = radio_AutoLogin_Enabled.IsChecked == true ? 1 : 0;
        }
        private void SaveRememberLoginStatus()
        {
            helper.User.ClientUser.rememberme = radio_RememberLoginInf_Enabled.IsChecked == true ? 1 : 0;
        }

        private void button_Logout_Click(object sender, RoutedEventArgs e)
        {
            helper.User.Reset();
            helper.Application.CloseApp();
        }
    }
}
