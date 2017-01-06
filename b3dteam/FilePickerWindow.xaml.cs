using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace b3dteam
{
    /// <summary>
    /// Interaction logic for FilePickerWindow.xaml
    /// </summary>
    public partial class FilePickerWindow : Window
    {
        public delegate void pickerResultChanged(bool result);
        public event pickerResultChanged OnPickerResultChanged;

        public string Ball3DPath = string.Empty;

        public FilePickerWindow()
        {
            InitializeComponent();

            if (File.Exists(textbox_FilePath.Text))
            {
                Ball3DPath = textbox_FilePath.Text;
                button_Accept.IsEnabled = true;
            }
        }

        private void button_Accept_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button_FilePicker_Click(object sender, RoutedEventArgs e)
        {
            OnPickerResultChanged?.Invoke(AskUserForBall3DExeFile());
        }

        public bool AskUserForBall3DExeFile()
        {
            Microsoft.Win32.OpenFileDialog openPicker = new Microsoft.Win32.OpenFileDialog()
            {
                DefaultExt = ".exe",
                Filter = "Ball 3D|Ball 3D.exe;"
            };

            bool? result = openPicker.ShowDialog();

            if (result == true)
            {
                Ball3DPath = textbox_FilePath.Text = openPicker.FileName.ToString();
                button_Accept.IsEnabled = true;
                return true;
            }
            else
            {
                Ball3DPath = textbox_FilePath.Text = "You have to select a file!.";
                button_Accept.IsEnabled = false;
                return false;
            }
        }

    }
}
