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
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class General : UserControl
    {
        public General()
        {
            InitializeComponent();

            FillCheckBoxes();
        }

        private void FillCheckBoxes()
        {
            if(Properties.Settings.Default.chat_sound_enabled == true)
            {
                checkbox_MessageNotyfication_Sound.IsChecked = true;
            }

            if(Properties.Settings.Default.chat_toast_enabled == true)
            {
                checkbox_MessageNotyfication_Toast.IsChecked = true;
            }
        }

        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.chat_toast_enabled = checkbox_MessageNotyfication_Toast.IsChecked == true ? true : false;
            Properties.Settings.Default.chat_sound_enabled = checkbox_MessageNotyfication_Sound.IsChecked == true ? true : false;

            OptionsWindow.gui.Close();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow.gui.Close();
        }
    }
}
