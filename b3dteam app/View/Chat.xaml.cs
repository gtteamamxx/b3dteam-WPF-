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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace b3dteam_app.View
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : UserControl
    {
        #region Variables
        public static Discord.DiscordClient _DiscordClient;
        public static bool IsLogged => _DiscordClient.CurrentUser != null && _DiscordClient.Servers.FirstOrDefault(p => p.Name.Contains("Ball 3D")) != null;
        public static ObservableCollection<Model.Server> ListOfServers;

        public static Discord.Server GetBall3DServer() => _DiscordClient.Servers.FirstOrDefault(p => p.Name.Contains("Ball 3D"));
        public System.Windows.Forms.RichTextBox GetRichTextBox;

        public bool isSending = false;

        public static Chat gui;
        #endregion

        public Chat()
        {
            InitializeComponent();

            #region Initializing variables
            gui = this;
            _DiscordClient = new Discord.DiscordClient(); ;
            ListOfServers = new ObservableCollection<Model.Server>();
            GetRichTextBox = textbox_Chat.Child as System.Windows.Forms.RichTextBox;

            textbox_Login.Text = helper.User.ClientUser.email;
            #endregion

            this.Loaded += (s, e) =>
            {
                GetRichTextBox.SelectionStart = GetRichTextBox.Text.Length;
                GetRichTextBox.ScrollToCaret();
            };

            listview_Servers.SelectionChanged += Listview_Servers_SelectionChanged;

            if (Properties.Settings.Default.autologin && !string.IsNullOrEmpty(Properties.Settings.Default.password))
            {
                textbox_Login.Text = Properties.Settings.Default.email;
                textbox_Passsword.Password = Properties.Settings.Default.password;
                button_Login_Click(button_Login, null);
            }

            #region Attachment Click
            GetRichTextBox.MouseClick += GetRichTextBox_MouseClick;
        }

        public static void GetRichTextBox_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var richTextBox = sender as System.Windows.Forms.RichTextBox;
            var lineNumber = richTextBox.Text.Substring(0, richTextBox.SelectionStart).Count(chr => chr == '\n');

            if(richTextBox.Lines.Length == 0)
            {
                return;
            }

            var text = richTextBox.Lines[lineNumber];

            if (text.Contains("Click here to check attachments#"))
            {
                ulong idOfMessage = 0;

                if (ulong.TryParse(text.Split('#')[1], out idOfMessage))
                {
                    Discord.Message message = null;

                    if (richTextBox.Name == "public")
                    {
                        message = GetBall3DServer().AllChannels.First(p => (gui.listview_Servers.SelectedItem as Model.Server).Name == p.Name).GetMessage(idOfMessage);
                    }
                    else
                    {
                        var server = ChatUtilities.PrivateMessage.gui.listview_PrivateChannels.SelectedItem as Model.Server;
                        message = _DiscordClient.GetChannel(server.Id).GetMessage(idOfMessage);
                    }

                    if (message.Attachments.Length == 0)
                    {
                        MessageBox.Show("Error occured", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var window = new Window()
                    {
                        Title = $"{message.Attachments[0].Filename} | Right mouse click -> Copy url",
                        Width = message.Attachments[0].Width == null ? 300 : (double)message.Attachments[0].Width,
                        Height = message.Attachments[0].Height == null ? 300 : (double)message.Attachments[0].Height,
                    };

                    if (message.Attachments[0].Width != null)
                    {
                        var image = new Image() { Source = new BitmapImage(new Uri(message.Attachments[0].Url)) };

                        window.MouseRightButtonDown += (s, f) =>
                        {
                            Clipboard.SetText(message.Attachments[0].Url);
                            MessageBox.Show("URL of file was coppied to clipboard");
                        };

                        window.Content = image;
                    }
                    else
                    {
                        var grid = new Grid();

                        grid.Children.Add(new TextBlock()
                        {
                            Text = $"File: {message.Attachments[0].Filename} {message.Attachments[0].Size / 1024 / 1024} Mb",
                            Margin = new Thickness(10),
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Center
                        });

                        var button = new Button()
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Content = "Download this file"
                        };

                        button.Click += (s, f) =>
                        {
                            System.Diagnostics.Process.Start(message.Attachments[0].Url);
                        };
                        grid.Children.Add(button);

                        window.MouseRightButtonDown += (s, f) =>
                        {
                            Clipboard.SetText(message.Attachments[0].Url);
                            MessageBox.Show("URL of file was coppied to clipboard");
                        };

                        window.Content = grid;
                    }

                    window.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Bad message", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region Changing server
        private async void Listview_Servers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isSending)
            {
                return;
            }

            GetRichTextBox.Clear();

            var selectedItem = listview_Servers.SelectedItem as Model.Server;

            if (selectedItem == null)
            {
                selectedItem = ListOfServers.First(p => p.Name == "global") as Model.Server;
            }

            listview_Servers.SelectedItem = selectedItem;
            var channelId = selectedItem.Id;

            var messagesFromChannel = await DownloadLastMessages(channelId);

            messagesFromChannel.Reverse();
            messagesFromChannel.ToList().ForEach(p => AddMessage(GetRichTextBox, p));

            SetUnreadedMessageStatus(selectedItem.Name, false);
        }
        #endregion

        #region Adding message to RichTextBox

        private DateTime _LastMessageTime = new DateTime();

        public static void AddMessage(System.Windows.Forms.RichTextBox richTextBox, Discord.Message message)
        {
            var author = message.User == null ? "Unknown user" : message.User.Name;

            gui.CheckLastMessageTimeAndAddDateLineIfNeeded(richTextBox, message);
            richTextBox.AppendText($"{message.Timestamp.ToShortTimeString()}");
            gui.AppendTextToRichTextBoxForms(richTextBox, $" {author}:", gui.GetColorOfNick(author), false);

            bool hasAttachments = message.Attachments.Length > 0;

            if (hasAttachments)
            {
                gui.AppendTextToRichTextBoxForms(richTextBox, $" Click here to check attachments#{message.Id}", System.Drawing.Color.Red, false);
            }

            richTextBox.AppendText($" {message.Text}\n");
            richTextBox.SelectionStart = richTextBox.Text.Length;
            richTextBox.ScrollToCaret();
        }

        private void CheckLastMessageTimeAndAddDateLineIfNeeded(System.Windows.Forms.RichTextBox richTextBox, Discord.Message message)
        {
            if (_LastMessageTime.Day != message.Timestamp.Day)
            {
                AppendTextToRichTextBoxForms(richTextBox, $"=================={message.Timestamp.ToShortDateString()}==================", System.Drawing.Color.Red, true);
                _LastMessageTime = message.Timestamp;
            }
        }

        //from so
        public void AppendTextToRichTextBoxForms(System.Windows.Forms.RichTextBox richTextBox, string text, System.Drawing.Color color, bool AddNewLine = false)
        {
            if (AddNewLine)
            {
                text += Environment.NewLine;
            }

            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;

            richTextBox.SelectionColor = color;
            richTextBox.AppendText(text);
            richTextBox.SelectionColor = richTextBox.ForeColor;
        }

        #endregion

        #region Logging
        private static bool _MessageReceivedSubscribed = false;
        private static bool _OnClientStatusChangedSubscribed = false;

        private async void button_Login_Click(object sender, RoutedEventArgs e)
        {
            var defaultText = button_Login.Content as string;
            button_Login.IsEnabled = false;
            button_Login.Content = "Logging...";

            try
            {
                await _DiscordClient.Connect(textbox_Login.Text, textbox_Passsword.Password);
            }
            catch
            {
                MessageBox.Show("Bad login or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                button_Login.IsEnabled = true;
                button_Login.Content = defaultText;
                return;
            }

            var userServers = _DiscordClient.Servers;

            if (userServers == null)
            {
                MessageBox.Show("Your profile doesn't have any servers. Check that you are connected to Ball3D Chat on Discord", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                button_Login.IsEnabled = true;
                button_Login.Content = defaultText;
                return;
            }

            var ball3DServer = GetBall3DServer();

            if (ball3DServer != null && ball3DServer.AllChannels.Any(p => p.Name.Contains("global")))
            {
                _DiscordClient.SetStatus(helper.User.ClientStatus == helper.SQLManager.Ball3D_Status.Status_Online ? Discord.UserStatus.Online : Discord.UserStatus.Invisible);

                if (_OnClientStatusChangedSubscribed == false)
                {
                    _OnClientStatusChangedSubscribed = true;
                    helper.User.OnClientStatusChanged += (newStatus, oldStatus) =>
                    {
                        if (newStatus != oldStatus)
                        {
                            _DiscordClient.SetStatus(helper.User.ClientStatus == helper.SQLManager.Ball3D_Status.Status_Online ? Discord.UserStatus.Online : Discord.UserStatus.Invisible);
                        }
                    };
                }

                Properties.Settings.Default.email = textbox_Login.Text;
                Properties.Settings.Default.password = textbox_Passsword.Password;

                if (checkbox_AutoLogin.IsChecked == true)
                {
                    Properties.Settings.Default.autologin = true;

                }
                Properties.Settings.Default.Save();

                ball3DServer.AllChannels
                    .Where(p => p.Type == Discord.ChannelType.Text && p.IsPrivate == false)
                        .ToList()
                            .ForEach(p => ListOfServers.Add(new Model.Server { Name = p.Name, Id = p.Id, MuteText = CheckIfChannelIsMuted(p.Name) ? "Unmute" : "Mute", UnreadedMessages = "0" }));

                listview_Servers.ItemsSource = ListOfServers;
                listview_Servers.SelectedItem = ListOfServers.First(p => p.Name == "global");

                if (_MessageReceivedSubscribed == false)
                {
                    _MessageReceivedSubscribed = true;
                    _DiscordClient.MessageReceived += _DiscordClient_MessageReceived;
                }
                ChangeButtonsVisibility(Visibility.Collapsed);
                ShowChatAndServers();
            }
            else
            {
                button_Login.IsEnabled = true;
                button_Login.Content = defaultText;
                MessageBox.Show($"Your profile isn't invited to \"Ball 3D\" server, or \"#global\" channel{Environment.NewLine}Click button below", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

        }
        #endregion

        #region Recieving message
        private void _DiscordClient_MessageReceived(object sender, Discord.MessageEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
               {
                   if ((listview_Servers.SelectedItem as Model.Server).Name == e.Channel.Name)
                   {
                       AddMessage(GetRichTextBox, e.Message);
                       if (e.User.Name != _DiscordClient.CurrentUser.Name)
                       {
                           MainWindow.PlaySound("Sounds/message_received.mp3");
                       }
                   }
                   else if (e.Channel.Users.Count() == 2) // private mesage
                   {
                       if (ChatUtilities.PrivateMessage.gui != null && ChatUtilities.PrivateMessage.gui.listview_PrivateChannels.SelectedItem as Model.Server != null)
                       {
                           var selectedItem = ChatUtilities.PrivateMessage.gui.listview_PrivateChannels.SelectedItem as Model.Server;

                           if (selectedItem.Name == e.Channel.Name)
                           {
                               AddMessage(ChatUtilities.PrivateMessage.GetRichTextBox, e.Message);

                               if (e.User.Name != _DiscordClient.CurrentUser.Name)
                               {
                                   MainWindow.PlaySound("Sounds/message_received.mp3");
                               }
                               
                               return;
                           }
                       }
                       int numOfUnreadedMessages = SetUnreadedMessageStatus(e.Channel.Name, true, true);

                       var server = new Model.Server()
                       {
                           Id = e.Channel.Id,
                           MuteText = string.Empty,
                           Name = e.Channel.Name,
                           UnreadedMessages = numOfUnreadedMessages.ToString()
                       };

                       ChatUtilities.PrivateMessage.AddPrivateChannelIfNeccessary(server);

                       Model.NotyficationHelper.SendMessage($"Private Message", $"{e.User.Name}: {(e.Message.Attachments.Length > 0 ? "Sends a file" : e.Message.Text)}");
                   }
                   else if (e.User.Name != _DiscordClient.CurrentUser.Name && !CheckIfChannelIsMuted(e.Channel.Name))
                   {
                       //notyify about received message
                       SetUnreadedMessageStatus(e.Channel.Name);
                       Model.NotyficationHelper.SendMessage($"{e.Channel.Name}", $"{e.User.Name}: {(e.Message.Attachments.Length > 0 ? "Sends a file" : e.Message.Text)}");
                   }
               });
        }
        #endregion

        #region Setting a message unreaded

        private List<Tuple<Grid, string>> _ListOfGridsInServersName = new List<Tuple<Grid, string>>();

        public int SetUnreadedMessageStatus(string channelName, bool add = true, bool isPrivateMessage = false, int numOfPrivateMessagesReaded = 0)
        {
            if (isPrivateMessage)
            {
                TextBlock textblockOfUnreadedPrivateMessages = button_PrivateMessages_UnreadedMessages_Grid.Children[0] as TextBlock;

                int numOfUnreadedPrivateMessages = -1;

                try
                {
                    numOfUnreadedPrivateMessages = int.Parse(textblockOfUnreadedPrivateMessages.Text);
                }
                catch
                {
                    numOfUnreadedPrivateMessages = 0;
                }

                if (add)
                {
                    numOfUnreadedPrivateMessages++;
                }
                else
                {
                    numOfUnreadedPrivateMessages -= numOfPrivateMessagesReaded;
                }

                if (numOfUnreadedPrivateMessages <= 0)
                {
                    button_PrivateMessages_UnreadedMessages_Grid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    button_PrivateMessages_UnreadedMessages_Grid.Visibility = Visibility.Visible;
                }

                textblockOfUnreadedPrivateMessages.Text = $"{numOfUnreadedPrivateMessages}";
                return numOfUnreadedPrivateMessages;
            }
            var gridWithText = _ListOfGridsInServersName.FirstOrDefault(p => p.Item2 == channelName);

            if (gridWithText == null)
            {
                foreach (var grid in FindVisualChildren<Grid>(this))
                {
                    if (grid.Children.Count == 4 && (grid.Children[1] is TextBlock) && (grid.Children[1] as TextBlock).Text == channelName)
                    {
                        SetUnreadedMessageStatusAtGrid(grid, add);
                        _ListOfGridsInServersName.Add(new Tuple<Grid, string>(grid, channelName));
                        break;
                    }
                }
            }
            else
            {
                SetUnreadedMessageStatusAtGrid(gridWithText.Item1, add);
            }

            return -1;
        }

        private void SetUnreadedMessageStatusAtGrid(Grid grid, bool add = true)
        {
            var gridWithTextBlockOfMessagesCount = (grid.Children[2] as Grid);
            var channel = ((grid.Children[3] as Button).DataContext as Model.Server);

            int numOfUnreadedMessages = add ? (int.Parse(channel.UnreadedMessages) + 1) : 0;

            gridWithTextBlockOfMessagesCount.Visibility = numOfUnreadedMessages == 0 ? Visibility.Collapsed : Visibility.Visible;

            channel.UnreadedMessages = $"{numOfUnreadedMessages}";
            (gridWithTextBlockOfMessagesCount.Children[0] as TextBlock).Text = $"{numOfUnreadedMessages}";
        }

        #endregion

        #region Visible Changes
        private void ShowChatAndServers()
        {
            textbox_Chat.Visibility = Visibility.Visible;
            listview_Servers.Visibility = Visibility.Visible;
            textbox_Message.Visibility = Visibility.Visible;
            stackpanel_Message.Visibility = Visibility.Visible;
            button_PrivateMessages.Visibility = Visibility.Visible;
            button_OnlineUsers.Visibility = Visibility.Visible;
        }

        private void HideChatAndServers()
        {
            textbox_Chat.Visibility = Visibility.Collapsed;
            listview_Servers.Visibility = Visibility.Collapsed;
            stackpanel_Message.Visibility = Visibility.Collapsed;
            button_PrivateMessages.Visibility = Visibility.Collapsed;
            button_OnlineUsers.Visibility = Visibility.Collapsed;
        }
        private void ChangeButtonsVisibility(Visibility visible)
        {
            textblock_Info.Visibility = visible;
            textbox_Login.Visibility = visible;
            textbox_Passsword.Visibility = visible;
            button_Login.Visibility = visible;
            button_Register.Visibility = visible;
            checkbox_AutoLogin.Visibility = visible;
        }
        #endregion

        #region Muting server

        private void button_ServerMute(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            bool mute = ((string)button.Content).Contains("Un");
            var server = ((button.DataContext) as Model.Server);
            button.Content = mute ? "Mute" : "Unmute";

            if (!mute)
            {
                Properties.Settings.Default.muttedservers += $"{server.Name}#";
            }
            else
            {
                Properties.Settings.Default.muttedservers = Properties.Settings.Default.muttedservers.Replace($"{server.Name}#", "");
            }

            button.Background = !mute ? new SolidColorBrush(new Color { A = 50, R = 255 }) : new SolidColorBrush(new Color { A = 50, G = 255 });
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Sending

        public async void textbox_Message_KeyUp(object sender, KeyEventArgs e)
        {
            await textbox_Message_KeyUpped(sender, e);
        }

        public async static Task textbox_Message_KeyUpped(object sender, KeyEventArgs e)
        {
            TextBox textbox = ((sender as TextBox).Name.Contains("vate") ? ChatUtilities.PrivateMessage.gui.textbox_MessagePrivate : gui.textbox_Message);

            bool isPrivate = textbox.Name.Contains("vate") ? true : false;

            if (gui.isSending)
            {
                return;
            }

            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift && !gui.isSending)
            {
                if (textbox.Text.Trim().Length == 0)
                {
                    return;
                }

                Discord.Channel actualChannel = null;

                if (!isPrivate)
                {
                    actualChannel = GetBall3DServer().AllChannels.First(p => (gui.listview_Servers.SelectedItem as Model.Server).Name == p.Name);
                }
                else
                {
                    var server = ChatUtilities.PrivateMessage.ListOfPrivateChannels.First(p => p.Item1.Id == (ChatUtilities.PrivateMessage.gui.listview_PrivateChannels.SelectedItem as Model.Server).Id);
                    actualChannel = _DiscordClient.GetChannel(server.Item1.Id);
                }

                gui.isSending = true;
                var message = await actualChannel.SendMessage(textbox.Text);
                gui.isSending = false;
                textbox.Text = "";
            }

            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (Clipboard.GetFileDropList().Count > 0)
                {
                    var file = Clipboard.GetFileDropList()[0];

                    Button button = isPrivate ? ChatUtilities.PrivateMessage.gui.button_SendFile : gui.button_SendFile;

                    button.IsEnabled = false;
                    button.Content = "Sending...";

                    var message = await gui.SendFile(button, file);

                    button.IsEnabled = true;
                    button.Content = "Send file";

                    if (message == null)
                    {
                        MessageBox.Show("There was problem with sending a file. Check if file size is less than 8.00 Mb", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        public async void button_SendFile_Click(object sender, RoutedEventArgs e)
        {
            await button_SendFile_Clicked(sender, e);
        }
        public async static Task button_SendFile_Clicked(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "All files|*.*" };

            var result = ofd.ShowDialog();

            if (result == false)
            {
                return;
            }
            var button = sender as Button;

            button.IsEnabled = false;
            button.Content = "Sending...";

            var message = await gui.SendFile(button, ofd.FileName);

            if (message == null)
            {
                MessageBox.Show("There was problem with sending a file. Check if file size is less than 8.00 Mb", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            button.IsEnabled = true;
            button.Content = "Send file";
        }

        private async Task<Discord.Message> SendFile(Button button, string path)
        {
            Discord.Message result = null;

            isSending = true;
            if (button == button_SendFile)
            {
                result = await GetBall3DServer().AllChannels.First(p => (listview_Servers.SelectedItem as Model.Server).Name == p.Name).SendFile(path);
            }
            else
            {
                result = await _DiscordClient.GetChannel((ChatUtilities.PrivateMessage.gui.listview_PrivateChannels.SelectedItem as Model.Server).Id).SendFile(path);
            }
            isSending = false;

            return result;
        }

        #endregion

        #region DownloadingMessages
        public async Task<List<Discord.Message>> DownloadLastMessages(ulong channelId)
        {
            isSending = true;
            var result = (await _DiscordClient.GetChannel(channelId).DownloadMessages()).ToList();
            isSending = false;
            return result;
        }
        #endregion

        #region Register
        private void button_Register_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ball3d.com/chat");
        }

        #endregion

        #region Utilities

        public void ResetPage()
        {
            _DiscordClient = new Discord.DiscordClient(); ;
            ListOfServers = new ObservableCollection<Model.Server>();
            textbox_Login.Text = helper.User.ClientUser.email;
            GetRichTextBox.Text = "";
            ChangeButtonsVisibility(Visibility.Visible);
            HideChatAndServers();
            button_Login.Content = "Login";
            button_Login.IsEnabled = true;
        }

        private static List<Tuple<string, System.Drawing.Color>> listOfNicksAndColors = new List<Tuple<string, System.Drawing.Color>>();

        public System.Drawing.Color GetColorOfNick(string nick)
        {
            var user = listOfNicksAndColors.FirstOrDefault(p => p.Item1 == nick);

            if (user == null)
            {
                var random = new Random();
                //not to light, and not to dark
                user = new Tuple<string, System.Drawing.Color>(nick, System.Drawing.Color.FromArgb(255, (byte)random.Next(30, 215), (byte)random.Next(30, 215), (byte)random.Next(30, 215)));
                listOfNicksAndColors.Add(user);
            }
            return user.Item2;
        }

        private bool CheckIfChannelIsMuted(string channelName)
        {
            return Properties.Settings.Default.muttedservers.Contains($"{channelName}#");
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
        #endregion

        #region Private messages

        private ChatUtilities.PrivateMessage _PrivateMessagesWindow = null;
        private void button_PrivateMessages_Click(object sender, RoutedEventArgs e)
        {
            if (_PrivateMessagesWindow == null)
            {
                var privateMessagesWindow = new ChatUtilities.PrivateMessage();

                privateMessagesWindow.Closed += (s, f) =>
                {
                    _PrivateMessagesWindow = null;
                };

                _PrivateMessagesWindow = privateMessagesWindow;
                privateMessagesWindow.Show();
            }
            else
            {
                _PrivateMessagesWindow.Show();
            }
        }
        #endregion

        #region Show online users
        private void button_OnlineUsers_Click(object sender, RoutedEventArgs e)
        {
            var onlineUsersWindow = new ChatUtilities.OnlineUsers();
            onlineUsersWindow.ShowDialog();
        }
        #endregion
    }
}
