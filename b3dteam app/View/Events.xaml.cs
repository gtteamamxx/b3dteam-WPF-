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
using Smith.WPF.HtmlEditor;

namespace b3dteam_app.View
{
    /// <summary>
    /// Interaction logic for Events.xaml
    /// </summary>
    public partial class Events : UserControl
    {
        public Events()
        {
            InitializeComponent();

            this.Loaded += async (s, e) =>
            {
                htmleditor_Editor.ContentHtml = await helper.SQLManager.GetPlainHTMLOfChanges();
            };
        }
    }
}
