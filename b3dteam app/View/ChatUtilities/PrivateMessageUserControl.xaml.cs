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

namespace b3dteam_app.View.ChatUtilities
{
    /// <summary>
    /// Interaction logic for PrivateMessageUserControl.xaml
    /// </summary>
    public partial class PrivateMessageUserControl : UserControl
    {
        public PrivateMessageUserControl()
        {
            InitializeComponent();

            this.DataContextChanged += PrivateMessageUserControl_DataContextChanged;
        }

        public void ChangeData(Model.Server server)
        {
            textblock_Name.Text = server.Name;

            int numOfUnreadedMessages = -1;
            int.TryParse(server.UnreadedMessages, out numOfUnreadedMessages);

            grid_UnreadedMessages.Visibility = numOfUnreadedMessages <= 0 ? Visibility.Collapsed : Visibility.Visible;

            if (grid_UnreadedMessages.Visibility == Visibility.Visible)
            {
                textblock_UnreadedMessages.Text = server.UnreadedMessages;
            }
        }
        private void PrivateMessageUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue != null)
            {
                PrivateMessage.SetUserControlToChannel(this, e.NewValue as Model.Server);
                ChangeData(e.NewValue as Model.Server);
            }
        }
    }
}
