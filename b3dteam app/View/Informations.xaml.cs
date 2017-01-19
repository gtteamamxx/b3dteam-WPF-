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

namespace b3dteam_app.View
{
    /// <summary>
    /// Interaction logic for Informations.xaml
    /// </summary>
    public partial class Informations : UserControl
    {
        public Informations()
        {
            InitializeComponent();

            this.Loaded += Informations_Loaded;
        }

        private async void Informations_Loaded(object sender, RoutedEventArgs e)
        {
            htmleditor_Editor.ContentHtml = (await helper.SQLManager.GetPlainHTMLOfInformations())??"";
        }
    }
}
