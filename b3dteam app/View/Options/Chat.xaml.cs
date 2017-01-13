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
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : UserControl
    {
        public Chat()
        {
            InitializeComponent();

            CheckIfRadioButtonsShouldBeChecked();
            FillTextBoxesWithData();
        }

        private void FillTextBoxesWithData()
        {
            textbox_Login.Text = Properties.Settings.Default.email;
            textbox_Password.Password = Properties.Settings.Default.password;

            textblock_CurrentNick.Text = (View.Chat._DiscordClient.CurrentUser == null) ? "Current name: Not logged in" : $"Current name: {View.Chat._DiscordClient.CurrentUser.Name}";
        }
        private void CheckIfRadioButtonsShouldBeChecked()
        {
            if(Properties.Settings.Default.autologin == true)
            {
                radio_Autologin_Enabledd.IsChecked = true;
                radio_Autologin_Disabledd.IsChecked = false;
            }
            else
            {
                radio_Autologin_Enabledd.IsChecked = false;
                radio_Autologin_Disabledd.IsChecked = true;
            }
        }
        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow.gui.Close();
        }

        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.autologin = radio_Autologin_Enabledd.IsChecked == true ? true : false;
            MessageBox.Show("Save completed", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            Properties.Settings.Default.Save();
        }

        private void button_Logout_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.autologin = false;
            Properties.Settings.Default.email = "";
            Properties.Settings.Default.password = "";
            Properties.Settings.Default.Save();
            MessageBox.Show("If you want to login on chat, go to Chat page", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            View.Chat.gui.ResetPage();
        }
    }
}
