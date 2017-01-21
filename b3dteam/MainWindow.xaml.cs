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

        public static helper.User ClientUser { get; set; }
        public static bool _RunOnlyApp = false;

        public MainWindow()
        {
            InitializeComponent();

            Ball3DGameProcess = new Ball3DProcess(this);
            NotyficationManager.SetInstanceOfMainWindow(this);

            text_Info.Content = "Loading...";

            this.Loaded += async (a, b) =>
            {
            ret:

                if (!await Network.IsInternetAvailable())
                {
                    var clickedButton = MessageBox.Show($"Before using this app, you must have internet connection! [pinging google.com failed] {Environment.NewLine}Click \"OK\" to retry.", "Information", MessageBoxButton.OKCancel);

                    if (clickedButton == MessageBoxResult.OK)
                    {
                        goto ret;
                    }
                    else
                    {
                        this.Close();
                    }
                }

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


            helper.User.OnClientSave += User_OnClientSave;
            helper.User.OnClientReset += User_OnClientReset;

            helper.Application.OnCloseApp += () =>
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            };
        }

        private void User_OnClientReset(helper.User user)
        {
            user.ResetUser();
        }

        private void User_OnClientSave(helper.User user)
        {
            user.SaveUser();
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
                button_statusOffine.Visibility = Visibility.Visible;
                button_statusOnline.Visibility = Visibility.Visible;
                button_selectFile.Visibility = Visibility.Collapsed;
                button_RunApp_Offine.Visibility = Visibility.Visible;
                button_RunApp_Online.Visibility = Visibility.Visible;

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
            button_RunApp_Offine.Visibility = Visibility.Collapsed;
            button_RunApp_Online.Visibility = Visibility.Collapsed;
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
            else if ((text_Info.Content as string).Contains("There was"))
            {
                Run(Ball3DStatus.ClientStatus);
                return;
            }
            else if ((text_Info.Content as string).Contains("account"))
            {
                Run(Ball3DStatus.ClientStatus);
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
            Run(helper.SQLManager.Ball3D_Status.Status_Online);
        }

        private void button_statusOffine_Click(object sender, RoutedEventArgs e)
        {
            Run(helper.SQLManager.Ball3D_Status.Status_Offine);
        }

        public async void Run(helper.SQLManager.Ball3D_Status status)
        {
            Ball3DStatus.ClientStatus = status;

            button_statusOffine.Visibility = Visibility.Collapsed;
            button_statusOnline.Visibility = Visibility.Collapsed;
            button_selectFile.Visibility = Visibility.Collapsed;
            button_RunApp_Offine.Visibility = Visibility.Collapsed;
            button_RunApp_Online.Visibility = Visibility.Collapsed;

            if (!(await CheckInternetConnection()) || !(await CheckSQLConnection()) || !CheckUserLogin())
            {
                return;
            }

            text_Info.Content = "";

            this.Hide();

            if (status == helper.SQLManager.Ball3D_Status.Status_Online)
            {
                Ball3DStatus.UpdateStatus(helper.SQLManager.Ball3D_Status.Status_Online);
            }

            Ball3DGameProcess.CheckBall3DProcessAndSendStatus();

            if (_RunOnlyApp)
            {
                ShowApplication();
            }
            else if (Ball3DGameProcess.IsBall3DProcessRunning())
            {
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

            if (!(await helper.SQLManager.ConnectToDatabase()))
            {
                text_Info.Content = $"There was problem with connection to database.{Environment.NewLine}Write to grs4_98@o2.pl";
                button_selectFile.Visibility = Visibility.Visible;
                button_statusOffine.Visibility = Visibility.Collapsed;
                button_statusOnline.Visibility = Visibility.Collapsed;
                button_selectFile.Content = "Try again";
                return false;
            }
            return true;
        }

        private bool _LoginWindowClosedEventSubscribed = false;

        public bool CheckUserLogin()
        {
            text_Info.Content = "Checking user...";

            if (ClientUser != null)
            {
                return true;
            }

            else if (Properties.Settings.Default.userid != -1 && Properties.Settings.Default.autologin == true)
            {
                ClientUser = helper.User.ClientUser = new helper.User()
                {
                    userid = Properties.Settings.Default.userid,
                    login = Properties.Settings.Default.login,
                    password = Properties.Settings.Default.password,
                    email = Properties.Settings.Default.email,
                    lastactivity = Properties.Settings.Default.lastactivity,
                    regtime = Properties.Settings.Default.regtime,
                    usertype = Properties.Settings.Default.usertype,
                    rememberme = Properties.Settings.Default.rememberme ? 1 : 0,
                    autologin = Properties.Settings.Default.autologin ? 1 : 0
                };

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
                            (myNotifyIcon.ContextMenu.Items[0] as MenuItem).IsEnabled = true;
                            Run(Ball3DStatus.ClientStatus);
                        }
                    };
                }

                loginWindow.ShowDialog();

                return false;
            }
        }

        private bool _Ball3DAppClosedSubscribed = false;
        private void ShowApplication()
        {
            b3dteam_app.MainWindow mw = b3dteam_app.MainWindow.gui == null ? new b3dteam_app.MainWindow() : b3dteam_app.MainWindow.gui;

            if (!_Ball3DAppClosedSubscribed)
            {
                mw.Closed += (s, e) =>
                {
                    b3dteam_app.MainWindow.gui = null;
                    helper.Application.IsAppRunning = false;
                    _Ball3DAppClosedSubscribed = false;
                    (myNotifyIcon.ContextMenu.Items[0] as MenuItem).IsEnabled = true;
                };
            }

            if (mw != null)
            {
                helper.Application.IsAppRunning = true;
                mw.Show();
            }
        }

        //////////////////////////////////////////////////////////////////////////

        private void ContextMenu_statusOnline_Click(object sender, RoutedEventArgs e)
        {
            Ball3DStatus.ClientStatus = helper.SQLManager.Ball3D_Status.Status_Online;
        }

        private void ContextMenu_statusOffine_Click(object sender, RoutedEventArgs e)
        {
            Ball3DStatus.ClientStatus = helper.SQLManager.Ball3D_Status.Status_Offine;
        }

        private void ContextMenu_exitApplication_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ContextMenu_showApp_Click(object sender, RoutedEventArgs e)
        {
            if (ClientUser != null)
            {
                ShowApplication();
            }
            else
            {
                MessageBox.Show("Before opening app, you have login first!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //////////////////////////////////////////////////////////////////////////

        private void button_RunApp_Offine_Click(object sender, RoutedEventArgs e)
        {
            _RunOnlyApp = true;
            Run(helper.SQLManager.Ball3D_Status.Status_Offine);
        }

        private void button_RunApp_Online_Click(object sender, RoutedEventArgs e)
        {
            _RunOnlyApp = true;
            Run(helper.SQLManager.Ball3D_Status.Status_Online);
        }
    }
}
