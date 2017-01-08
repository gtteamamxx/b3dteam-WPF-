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
        public static User ClientUser;

        public MainWindow()
        {
            InitializeComponent();

            Ball3DGameProcess = new Ball3DProcess(this);
            NotyficationManager.SetInstanceOfMainWindow(this);

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
                    else
                    {
                        MessageBox.Show("There is problem with selected Ball3D.exe file. Run application again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Properties.Settings.Default.Reset();
                        this.Close();
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

            AskUserForStatus();
        }

        public void AskUserForStatus()
        {
            if (File.Exists(Properties.Settings.Default.Ball3DExePath))
            {
                this.Height = 150;

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
                if (Ball3DPath == string.Empty || Ball3DPath == "None")
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
            if ((text_Info.Content as string).Contains("internet"))
            {
                AskUserForStatus();
                return;
            }
            else if((text_Info.Content as string).Contains("There was"))
            {
                RunGameWithStatus(Ball3DStatus.ClientStatus);
                return;
            }
            else if((text_Info.Content as string).Contains("account"))
            {
                RunGameWithStatus(Ball3DStatus.ClientStatus);
                return;
            }

            SelectBall3DFile();

            if (IsBall3DFileCorrect)
            {
                button_selectFile.Visibility = Visibility.Collapsed;
                text_Info.Content = "";
                SaveBall3DPathAndAskUserForStatus();
            }
            else
            {
                MessageBox.Show("There is problem with selected Ball3D.exe file. Run application again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Properties.Settings.Default.Reset();
                this.Close();
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
            Ball3DStatus.ClientStatus = status;

            button_statusOffine.Visibility = Visibility.Collapsed;
            button_statusOnline.Visibility = Visibility.Collapsed;
            button_selectFile.Visibility = Visibility.Collapsed;

            if (!(await CheckInternetConnection()) || !(await CheckSQLConnection()) || !CheckUserLogin())
            {
                return;
            }

            text_Info.Content = "";

            this.Hide();

            if (status == Ball3DStatus.Ball3D_Status.Status_Online)
            {
                Ball3DStatus.UpdateStatus(Ball3DStatus.Ball3D_Status.Status_Online);
            }

            if (Ball3DGameProcess.IsBall3DProcessRunning())
            {
                if (status == Ball3DStatus.Ball3D_Status.Status_Online)
                {
                    Ball3DGameProcess.CheckBall3DProcessAndSendStatus();
                }
                return;
            }
            else
            {
                Ball3DGameProcess.RunGame();
            }
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
                text_Info.Content = "There was problem with connection to database." + Environment.NewLine + "Write to grs4_98@o2.pl";
                button_selectFile.Visibility = Visibility.Visible;
                button_statusOffine.Visibility = Visibility.Collapsed;
                button_statusOnline.Visibility = Visibility.Collapsed;
                button_selectFile.Content = "Try again";
                return false;
            }
            return true;
        }

        private bool _LoginWindowClosedEventSubscribed = false;

        private bool CheckUserLogin()
        {
            text_Info.Content = "Checking user...";

            if (ClientUser != null)
            {
                return true;
            }
            else if (Properties.Settings.Default.userid != -1 && Properties.Settings.Default.autologin == true)
            {
                ClientUser = new User(Properties.Settings.Default.userid,
                    Properties.Settings.Default.login, Properties.Settings.Default.password, 
                    Properties.Settings.Default.email, Properties.Settings.Default.lastactivity, 
                    Properties.Settings.Default.regtime,Properties.Settings.Default.usertype);

                return true;
            }
            else
            {
                text_Info.Content = "You have to login to an account first!";
                button_selectFile.Visibility = Visibility.Visible;
                button_selectFile.Content = "Login";

                var loginWindow = new View.LoginWindow();

                if (!_LoginWindowClosedEventSubscribed)
                {
                    _LoginWindowClosedEventSubscribed = true;

                    loginWindow.Closed += (s, e) =>
                    {
                        if (ClientUser != null)
                        {
                            RunGameWithStatus(Ball3DStatus.ClientStatus);
                        }
                    };
                }

                loginWindow.ShowDialog();

                return false;
            }
        }

        //////////////////////////////////////////////////////////////////////////

        private void ContextMenu_statusOnline(object sender, RoutedEventArgs e)
        {
            Ball3DStatus.ClientStatus = Ball3DStatus.Ball3D_Status.Status_Online;
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
