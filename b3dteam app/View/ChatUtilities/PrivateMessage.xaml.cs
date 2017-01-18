using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace b3dteam_app.View.ChatUtilities
{
    /// <summary>
    /// Interaction logic for PrivateMessage.xaml
    /// </summary>
    public partial class PrivateMessage : Window
    {
        public static ObservableCollection<Model.Server> ListOfPrivateChannels = new ObservableCollection<Model.Server>();
        public static PrivateMessage gui;

        public static void AddPrivateChannelIfNeccessary(Model.Server server)
        {
            if(ListOfPrivateChannels == null || server == null)
            {
                return;
            }

            var result = ListOfPrivateChannels.FirstOrDefault(p => p.Id == server.Id);

            if(result == null)
            {
                ListOfPrivateChannels.Add(server);
            }
            else
            {
                gui.CheckIfGridShouldBeHiden(server);
            }
        }

        public PrivateMessage()
        {
            InitializeComponent();
            gui = this;

            this.Loaded += PrivateMessage_Loaded;
            listview_PrivateChannels.SelectionChanged += Listview_PrivateChannels_SelectionChanged;
        }

        private void Listview_PrivateChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = listview_PrivateChannels.SelectedItem as Model.Server;
            int numOfUnreadedMessages = -1;

            try
            {
                numOfUnreadedMessages = int.Parse(selectedItem.UnreadedMessages);
            }
            catch {}
            //ShowChat
            
            if (numOfUnreadedMessages != -1)
            {
                selectedItem.UnreadedMessages = "";
                Chat.gui.SetUnreadedMessageStatus(selectedItem.Name, false, true, numOfUnreadedMessages);
                CheckIfGridShouldBeHiden(selectedItem);
            }
        }

        private void PrivateMessage_Loaded(object sender, RoutedEventArgs e)
        {
            listview_PrivateChannels.Items.Clear();

            ListOfPrivateChannels.ToList().ForEach(p =>
            {
                listview_PrivateChannels.Items.Add(p);
                CheckIfGridShouldBeHiden(p);
            });
        }

        private void CheckIfGridShouldBeHiden(Model.Server server)
        {
            foreach (Grid grid in Chat.FindVisualChildren<Grid>(this))
            {
                if (grid.Children.Count == 2 && grid.Children[0] is TextBlock && grid.Children[1] is Grid)
                {
                    if ((grid.Children[0] as TextBlock).Text == server.Name)
                    {
                        if (string.IsNullOrEmpty(server.UnreadedMessages))
                        {
                            grid.Children[1].Visibility = Visibility.Collapsed;
                            ((grid.Children[1] as Grid).Children[0] as TextBlock).Text = "";
                        }
                        else
                        {
                            grid.Children[1].Visibility = Visibility.Visible;
                            ((grid.Children[1] as Grid).Children[0] as TextBlock).Text = $"{server.UnreadedMessages}";
                        }
                        ListOfPrivateChannels[ListOfPrivateChannels.IndexOf(ListOfPrivateChannels.First(p => p.Id == server.Id))] = server;
                    }
                }
            }
        }
    }
}
