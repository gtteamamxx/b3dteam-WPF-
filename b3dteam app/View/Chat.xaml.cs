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
        public Chat()
        {
            InitializeComponent();
            _DiscordClient = new Discord.DiscordClient(); ;
            ListOfServers = new ObservableCollection<Model.Server>();
            GetRichTextBox  = textbox_Chat.Child as System.Windows.Forms.RichTextBox;
            listview_Servers.SelectionChanged += Listview_Servers_SelectionChanged;

            textbox_Login.Text = helper.User.ClientUser.email;
        }

        private async void Listview_Servers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetRichTextBox.Clear();

            var channelId = (listview_Servers.SelectedItem as Model.Server).Id;

            var messagesFromChannel = await DownloadLastMessages(channelId);

            messagesFromChannel.Reverse();
            messagesFromChannel.ToList()
                    .ForEach(p =>
                    {
                        AddMessage(p);
                    });

            System.Windows.Forms.RichTextBox a = new System.Windows.Forms.RichTextBox();
        }

        private void AddMessage(Discord.Message message)
        {
            var author = message.User == null ? "Unknown user" : message.User.Name;

            GetRichTextBox.AppendText($"{message.Timestamp.ToShortTimeString()}");
            AppendTextToRichTextBoxForms($" {author}:", GetColorOfNick(author), false);

            GetRichTextBox.AppendText($" {message.Text}\n");
            GetRichTextBox.SelectionStart = GetRichTextBox.Text.Length;
            GetRichTextBox.ScrollToCaret();
        }
        /*public string FormatText(Discord.Message message)
        {
            List<string> idies = new List<string>();
            var text = message.Text;

            int temp_id = -1;

            for (int i = 0; i < text.Length; i++)
            {
                if(temp_id != -1)
                {
                    int result = 0;

                    if (int.TryParse($"{text[i]}", out result))
                    {
                        continue;
                    }
                    else
                    {
                        idies.Add(text.Substring(temp_id, i - temp_id));
                        temp_id = -1;
                    }
                }
                
                if ((i >= 2 && text[i-2] == '<' && text[i - 1] == '@'))
                {
                    temp_id = i;
                }
               
            }
            
            foreach(string id in idies)
            {
                var user = message.Channel.Users.First(p => p.Id == ulong.Parse(id));
                text.Replace($"<@{id}>", user.Name);
                var a = GetBall3DServer().Users;
            }

            return text;
        }*/
        private static List<Tuple<string, System.Drawing.Color>> listOfNicksAndColors = new List<Tuple<string, System.Drawing.Color>>();

        public System.Drawing.Color GetColorOfNick(string nick)
        {
            var user = listOfNicksAndColors.FirstOrDefault(p => p.Item1 == nick);

            if(user == null)
            {
                var random = new Random();
                user = new Tuple<string, System.Drawing.Color>(nick, System.Drawing.Color.FromArgb(255, (byte)random.Next(0, 150), (byte)random.Next(0, 150), (byte)random.Next(0, 150)));
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

                ball3DServer.AllChannels
                    .Where(p => p.Type == Discord.ChannelType.Text)
                        .ToList()
                            .ForEach(p => ListOfServers.Add(new Model.Server { Name = p.Name, Id = p.Id }));

                listview_Servers.ItemsSource = ListOfServers;
                listview_Servers.SelectedItem = ListOfServers.First(p => p.Name == "global");

                _DiscordClient.MessageReceived += _DiscordClient_MessageReceived;
                RemoveButtons();
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
        private void RemoveButtons()
        {
            textblock_Info.Visibility = Visibility.Collapsed;
            textbox_Login.Visibility = Visibility.Collapsed;
            textbox_Passsword.Visibility = Visibility.Collapsed;
            button_Login.Visibility = Visibility.Collapsed;
            button_Register.Visibility = Visibility.Collapsed;
            textbox_Message.Visibility = Visibility.Collapsed;
        }
        private void button_Register_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/HXXWDxS");
        }

        private static bool _Sending_Message = false;
        private async void textbox_Message_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !_Sending_Message)
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
    }
}
