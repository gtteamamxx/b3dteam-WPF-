using b3dteam.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace b3dteam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Ball3DPath = string.Empty;
        public bool IsBall3DFileCorrect => Ball3DPath.Contains(".exe");

        public Ball3DProcess Ball3DGameProcess;
        public User ClientUser;

        public MainWindow()
        {
            InitializeComponent();

            Ball3DGameProcess = new Ball3DProcess(this);

            this.Loaded += (a, b) =>
            {
                var ball3dExePath = Properties.Settings.Default.Ball3DExePath;

                if (ball3dExePath == "None" || ball3dExePath == "")
                {
                    SelectBall3DFile();

                    if (IsBall3DFileCorrect)
                    {
                        button_selectFile.Visibility = Visibility.Collapsed;
                        text_Info.Content = "";
                        SaveBall3DPathAndAskUserForStatus();
                    }
                }
                else
                {
                    AskUserForStatus();
                }
            };
        }
        public void SaveBall3DPathAndAskUserForStatus()
        {
            Properties.Settings.Default.Ball3DExePath = @Ball3DPath;
            Properties.Settings.Default.Save();

            Properties.Settings.Default.PropertyChanged += (s, e) =>
            {
                AskUserForStatus();
            };
        }

        public void AskUserForStatus()
        {
            if (File.Exists(Properties.Settings.Default.Ball3DExePath))
            {
                this.Height = 125;

                button_statusOffine.Visibility = Visibility.Visible;
                button_statusOnline.Visibility = Visibility.Visible;
                button_selectFile.Visibility = Visibility.Collapsed;

                text_Info.Content = "Do you want to set status to 'online'?";
            }
            else
            {
                SelectBall3DFile();
            }
        }

        public void SelectBall3DFile()
        {
            button_statusOffine.Visibility = Visibility.Collapsed;
            button_statusOnline.Visibility = Visibility.Collapsed;
            button_selectFile.Visibility = Visibility.Visible;

            text_Info.Content = "Waiting for select 'Ball 3D.exe'...";

            var filePickerWindow = new View.FilePickerWindow();

            filePickerWindow.Closed += (s, e) =>
            {
                if (Ball3DPath == string.Empty)
                {
                    if (filePickerWindow.Ball3DPath == string.Empty || filePickerWindow.Ball3DPath.Contains("You have"))
                    {
                        text_Info.Content = "You have to select 'Ball 3D.exe'! first!";
                    }
                    else
                    {
                        Ball3DPath = @filePickerWindow.Ball3DPath;
                    }
                }
            };

            filePickerWindow.ShowDialog();
        }

        private void button_selectFile_Click(object sender, RoutedEventArgs e)
        {
            if((text_Info.Content as string).Contains("internet"))
            {
                AskUserForStatus();
                return;
            }

            SelectBall3DFile();

            if (IsBall3DFileCorrect)
            {
                button_selectFile.Visibility = Visibility.Collapsed;
                text_Info.Content = "";
                SaveBall3DPathAndAskUserForStatus();
            }
        }

        private void button_statusOnline_Click(object sender, RoutedEventArgs e)
        {
            RunGameWithStatus(Ball3DStatus.Ball3D_Status.Status_Online);
        }

        private void button_statusOffine_Click(object sender, RoutedEventArgs e)
        {
            RunGameWithStatus(Ball3DStatus.Ball3D_Status.Status_Offine);
        }

        public async void RunGameWithStatus(Ball3DStatus.Ball3D_Status status)
        {
            button_statusOffine.Visibility = Visibility.Collapsed;
            button_statusOnline.Visibility = Visibility.Collapsed;
            button_selectFile.Visibility = Visibility.Collapsed;

            if (!(await CheckInternetConnection()) || !(await CheckSQLConnection()) || !CheckUserLogin())
            {
                return;
            }

            text_Info.Content = "";

            this.Hide();

            Ball3DStatus.ClientStatus = status;

            if (Ball3DGameProcess.IsBall3DProcessRunning())
            {
                if(status == Ball3DStatus.Ball3D_Status.Status_Online)
                {
                    Ball3DStatus.UpdateStatus(Ball3DStatus.Ball3D_Status.Status_Online);
                    Ball3DGameProcess.CheckBall3DProcessAndSendStatus();
                }
                return;
            }

            Ball3DGameProcess.RunGame();
        }

        private async Task<bool> CheckInternetConnection()
        {
            text_Info.Content = "Checking internet connection...";

            if (!(await Network.IsInternetAvailable()))
            {
                text_Info.Content = "You don't have internet connection.";
                button_selectFile.Visibility = Visibility.Visible;
                button_statusOffine.Visibility = Visibility.Collapsed;
                button_statusOnline.Visibility = Visibility.Collapsed;
                button_selectFile.Content = "Try again";
                return false;
            }
            return true;
        }
        private async Task<bool> CheckSQLConnection()
        {
            text_Info.Content = "Checking database connection...";

            if (!(await SQLManager.ConnectToDatabase()))
            {
                text_Info.Content = "There was problem with connection with database. Write to grs4_98@o2.pl";
                button_selectFile.Visibility = Visibility.Visible;
                button_statusOffine.Visibility = Visibility.Collapsed;
                button_statusOnline.Visibility = Visibility.Collapsed;
                button_selectFile.Content = "Try again";
                return false;
            }
            return true;
        }
        
        private bool CheckUserLogin()
        {
            text_Info.Content = "Checking user...";

            if (Properties.Settings.Default.userid != -1)
            {
                ClientUser = new User(Properties.Settings.Default.userid,
                    Properties.Settings.Default.login, Properties.Settings.Default.password);

                return true;
            }
            else
            {
                text_Info.Content = "You have to login to an account first!";
                button_selectFile.Visibility = Visibility.Visible;
                button_selectFile.Content = "Login";

                var loginWindow = new View.LoginWindow();

                loginWindow.Closed += (s, e) =>
                {
                    button_selectFile.Visibility = Visibility.Collapsed;
                };

                loginWindow.ShowDialog();

                return false;
            }
        }

        //////////////////////////////////////////////////////////////////////////

        private void ContextMenu_statusOnline(object sender, RoutedEventArgs e)
        {
            Ball3DStatus.ClientStatus = Ball3DStatus.Ball3D_Status.Status_Offine;
        }

        private void ContextMenu_statusOffine(object sender, RoutedEventArgs e)
        {
            Ball3DStatus.ClientStatus = Ball3DStatus.Ball3D_Status.Status_Offine;
        }
            
        private void ContextMenu_exitApplication(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
