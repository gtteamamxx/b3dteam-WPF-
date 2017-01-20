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
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;

namespace b3dteam_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow gui;
        public MediaPlayer MediaPlayer;
        public static void PlaySound(string fileName)
        {
            try { gui.MediaPlayer.Open(new Uri(fileName, UriKind.Relative)); }
            catch { return; }
            gui.MediaPlayer.Play();
        }

        public MainWindow()
        {
            InitializeComponent();
            gui = this;

            this.Loaded += MainWindow_Loaded;

            helper.User.OnClientStatusChanged += (n, o) => UpdateTitle();

            MediaPlayer = new MediaPlayer();

            helper.Application.OnCloseApp += () => Application.Current.Shutdown();
        }

        private void UpdateTitle()
        {
            this.Title = $"Ball3D Team App - Logged as {helper.User.ClientUser.login} - {(helper.User.ClientStatus == helper.SQLManager.Ball3D_Status.Status_Offine ? "offine" : "online")}";
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if(helper.User.ClientUser == null)
            {
                this.Title = "Ball3D Team App - Bad user";
                MessageBox.Show("Before using it, you have to login!");
                this.Close();
            }
            else
            {
                UpdateTitle();
            }
        }

        private void Menu_File_HideApp_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Menu_File_ExitApp_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Menu_Status_Click(object sender, RoutedEventArgs e)
        {
            helper.User.ClientStatus = ((sender as MenuItem).Header as string).Contains("online") ? helper.SQLManager.Ball3D_Status.Status_Online : helper.SQLManager.Ball3D_Status.Status_Offine;
            UpdateTitle();
        }

        private void Menu_File_Options_Click(object sender, RoutedEventArgs e)
        {
            View.OptionsWindow optionsWindow = new View.OptionsWindow();
            optionsWindow.ShowDialog();
        }
    }
}
