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

            var filePickerWindow = new FilePickerWindow();

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

        public void RunGameWithStatus(Ball3DStatus.Ball3D_Status status)
        {
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

        public void ExitApplicationWithOffineStatus()
        {
            Ball3DStatus.UpdateStatus(Ball3DStatus.Ball3D_Status.Status_Offine);
            this.Close();
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
            ExitApplicationWithOffineStatus();
        }
    }
}
