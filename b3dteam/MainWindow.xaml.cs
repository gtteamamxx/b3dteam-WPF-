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

namespace b3dteam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Ball3DPath = string.Empty;
        public bool IsBall3DFileCorrect => Ball3DPath.Contains(".exe");

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += (a, b) =>
            {
                var ball3dExePath = Properties.Settings.Default.Ball3DExePath;

                if (ball3dExePath == "None")
                {
                    SelectBall3DFile();

                    if (IsBall3DFileCorrect)
                    {
                        button_selectFile.Visibility = Visibility.Collapsed;
                        text_Info.Content = "";
                    }
                }
                else
                {
                    //AskUserForStatus();
                }
            };
        }

        public void SelectBall3DFile()
        {
            text_Info.Content = "Waiting for select 'Ball 3D.exe'...";

            var filePickerWindow = new FilePickerWindow();

            filePickerWindow.OnPickerResultChanged += (result) =>
            {
                if (result)
                {
                    Ball3DPath = filePickerWindow.Ball3DPath;
                }
            };

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
                        Ball3DPath = filePickerWindow.Ball3DPath;
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
            }
        }
    }
}
