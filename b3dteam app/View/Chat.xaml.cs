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

        public static Discord.DiscordClient _DiscordClient;
        public static bool IsLogged => _DiscordClient.CurrentUser != null && _DiscordClient.Servers.FirstOrDefault(p => p.Name.Contains("Ball 3D")) != null;
        public static ObservableCollection<Model.Server> ListOfServers;

        public static Discord.Server GetBall3DServer() => _DiscordClient.Servers.FirstOrDefault(p => p.Name.Contains("Ball 3D"));
        public System.Windows.Forms.RichTextBox GetRichTextBox;

        public static Chat gui;
        public Chat()
        {
            InitializeComponent();
            gui = this;
            _DiscordClient = new Discord.DiscordClient(); ;
            ListOfServers = new ObservableCollection<Model.Server>();
            GetRichTextBox  = textbox_Chat.Child as System.Windows.Forms.RichTextBox;

            listview_Servers.SelectionChanged += Listview_Servers_SelectionChanged;

            textbox_Login.Text = helper.User.ClientUser.email;

            if(Properties.Settings.Default.autologin && !string.IsNullOrEmpty(Properties.Settings.Default.password))
            {
                textbox_Login.Text = Properties.Settings.Default.email;
                textbox_Passsword.Password = Properties.Settings.Default.password;
                button_Login_Click(button_Login, null);
            }

            this.Loaded += (s, e) =>
            {
                GetRichTextBox.SelectionStart = GetRichTextBox.Text.Length;
                GetRichTextBox.ScrollToCaret();
            };
        }

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
        private async void Listview_Servers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetRichTextBox.Clear();

            var selectedItem = listview_Servers.SelectedItem as Model.Server;

            if(selectedItem == null)
            {
                selectedItem = ListOfServers.First(p => p.Name == "global") as Model.Server;
            }

            listview_Servers.SelectedItem = selectedItem;
            var channelId = selectedItem.Id;

            var messagesFromChannel = await DownloadLastMessages(channelId);

            messagesFromChannel.Reverse();
            messagesFromChannel.ToList().ForEach(p => AddMessage(p));

            System.Windows.Forms.RichTextBox a = new System.Windows.Forms.RichTextBox();
        }

        private DateTime _LastMessageTime = new DateTime();
        private void AddMessage(Discord.Message message)
        {
            var author = message.User == null ? "Unknown user" : message.User.Name;

            CheckLastMessageTimeAndAddDateLineIfNeeded(message);
            GetRichTextBox.AppendText($"{message.Timestamp.ToShortTimeString()}");
            AppendTextToRichTextBoxForms($" {author}:", GetColorOfNick(author), false);

            GetRichTextBox.AppendText($" {message.Text}\n");

            //scroll to end
            GetRichTextBox.SelectionStart = GetRichTextBox.Text.Length;
            GetRichTextBox.ScrollToCaret();
        }

        private void CheckLastMessageTimeAndAddDateLineIfNeeded(Discord.Message message)
        {
            if(_LastMessageTime.Day != message.Timestamp.Day)
            {
                AppendTextToRichTextBoxForms($"=================={message.Timestamp.ToShortDateString()}==================", System.Drawing.Color.Red, true);
                _LastMessageTime = message.Timestamp;
            }
        }

        private static List<Tuple<string, System.Drawing.Color>> listOfNicksAndColors = new List<Tuple<string, System.Drawing.Color>>();

        public System.Drawing.Color GetColorOfNick(string nick)
        {
            var user = listOfNicksAndColors.FirstOrDefault(p => p.Item1 == nick);

            if(user == null)
            {
                var random = new Random();
                //not to light, and not to dark
                user = new Tuple<string, System.Drawing.Color>(nick, System.Drawing.Color.FromArgb(255, (byte)random.Next(30, 215), (byte)random.Next(30, 215), (byte)random.Next(30, 215)));
                listOfNicksAndColors.Add(user);
            }
            return user.Item2;
        }

        //from so
        public void AppendTextToRichTextBoxForms(string text, System.Drawing.Color color, bool AddNewLine = false)
        {
            var box = GetRichTextBox;

            if (AddNewLine)
            {
                text += Environment.NewLine;
            }

            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        private async Task<List<Discord.Message>> DownloadLastMessages(ulong channelId)
        {
            return (await GetBall3DServer().AllChannels.First(p => p.Id == channelId).DownloadMessages()).ToList();
        }

        private static bool _OnClientStatusChangedSubscribed = false;
        private async void button_Login_Click(object sender, RoutedEventArgs e)
        {
            var defaultText = (sender as Button).Content as string;
            (sender as Button).IsEnabled = false;
            (sender as Button).Content = "Logging...";
        
            try
            {
                await _DiscordClient.Connect(textbox_Login.Text, textbox_Passsword.Password);
            }
            catch
            {
                MessageBox.Show("Bad login or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                (sender as Button).IsEnabled = true;
                (sender as Button).Content = defaultText;
                return;
            }

            var userServers = _DiscordClient.Servers;

            if(userServers == null)
            {
                MessageBox.Show("Your profile doesn't have any servers. Check that you are connected to Ball3D Chat on Discord", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                (sender as Button).IsEnabled = true;
                (sender as Button).Content = defaultText;
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
                    .Where(p => p.Type == Discord.ChannelType.Text)
                        .ToList()
                            .ForEach(p => ListOfServers.Add(new Model.Server { Name = p.Name, Id = p.Id, MuteText = CheckIfChannelIsMuted(p.Name) ? "Unmute" : "Mute" }));

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
                (sender as Button).IsEnabled = true;
                (sender as Button).Content = defaultText;
                MessageBox.Show($"Your profile isn't invited to \"Ball 3D\" server, or \"#global\" channel{Environment.NewLine}Click button below", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

        }

        private static bool _MessageReceivedSubscribed = false;

        private void _DiscordClient_MessageReceived(object sender, Discord.MessageEventArgs e)
        {
            this.Dispatcher.Invoke(
                () =>
                {
                    if ((listview_Servers.SelectedItem as Model.Server).Name == e.Channel.Name)
                    {
                        AddMessage(e.Message);
                    }
                });
        }

        private void ShowChatAndServers()
        {
            textbox_Chat.Visibility = Visibility.Visible;
            listview_Servers.Visibility = Visibility.Visible;
            textbox_Message.Visibility = Visibility.Visible;
        }

        private void HideChatAndServers()
        {
            textbox_Chat.Visibility = Visibility.Collapsed;
            listview_Servers.Visibility = Visibility.Collapsed;
            textbox_Message.Visibility = Visibility.Collapsed;
        }
        private void ChangeButtonsVisibility(Visibility visible)
        {
            textblock_Info.Visibility = visible;
            textbox_Login.Visibility = visible;
            textbox_Passsword.Visibility = visible;
            button_Login.Visibility = visible;
            button_Register.Visibility = visible;
            textbox_Message.Visibility = visible;
            checkbox_AutoLogin.Visibility = visible;
        }

        private void button_Register_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/HXXWDxS");
        }

        private static bool _Sending_Message = false;
        private async void textbox_Message_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.Key != Key.LeftShift && !_Sending_Message)
            {
                if (textbox_Message.Text.Trim().Length == 0)
                {
                    return;
                }

                var actualChannel = GetBall3DServer().AllChannels.First(p => (listview_Servers.SelectedItem as Model.Server).Name == p.Name);
                _Sending_Message = true;
                var message = await actualChannel.SendMessage(textbox_Message.Text);
                _Sending_Message = false;
                textbox_Message.Text = "";
            }
        }

        private void button_ServerMute(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            bool mute = ((string)button.Content).Contains("Un");
            var server = ((button.DataContext) as Model.Server);
            button.Content = mute ? "Mute" : "Unmute";

            if(!mute)
            {
                Properties.Settings.Default.muttedservers += $"{server.Name}#";
            }
            else
            {
                Properties.Settings.Default.muttedservers = Properties.Settings.Default.muttedservers.Replace($"{server.Name}#", "");
            }

            button.Background = !mute ? new SolidColorBrush(new Color { A = 50, R = 255 }) : 
                    new SolidColorBrush(new Color { A = 50, G = 255 });
            Properties.Settings.Default.Save();
        } 

        private bool CheckIfChannelIsMuted(string channelName)
        {
            return Properties.Settings.Default.muttedservers.Contains($"{channelName}#");
        }
    }
}
