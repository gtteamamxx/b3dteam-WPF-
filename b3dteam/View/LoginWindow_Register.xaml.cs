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

namespace b3dteam.View
{
    /// <summary>
    /// Interaction logic for LoginWindow_Register.xaml
    /// </summary>
    public partial class LoginWindow_Register : UserControl
    {
        private int _lastClickedTime;

        private Brush _DefualtTextBoxBackgroundBrush;

        public LoginWindow_Register()
        {
            InitializeComponent();
            _DefualtTextBoxBackgroundBrush = textbox_Password.Background;

            textbox_Email.TextChanged += TextBoxTextChanged;
            textbox_Login.TextChanged += TextBoxTextChanged;
            textbox_Password.PasswordChanged += TextBoxTextChanged;
            textbox_Password_Retype.PasswordChanged += TextBoxTextChanged;
        }

        private void TextBoxTextChanged(object sender, object e)
        {
            if (sender is TextBox)
            {
                (sender as TextBox).Background = _DefualtTextBoxBackgroundBrush;
            }
            else
            {
                (sender as PasswordBox).Background = _DefualtTextBoxBackgroundBrush;
            }
        }

        private void ResetColorsOnTextBoxes()
        {
            textbox_Email.Background = _DefualtTextBoxBackgroundBrush;
            textbox_Password_Retype.Background = _DefualtTextBoxBackgroundBrush;
            textbox_Login.Background = _DefualtTextBoxBackgroundBrush;
            textbox_Password.Background = _DefualtTextBoxBackgroundBrush;
        }

        private async void button_Register_Click(object sender, RoutedEventArgs e)
        {
            if(_lastClickedTime > Model.SQLManager.GetTimeStamp())
            {
                textblock_Info.Text = "You have to wait a while, before pressing 'Register' button!";
                textblock_Info.Foreground = new SolidColorBrush(Colors.Red);
            }

            _lastClickedTime = Model.SQLManager.GetTimeStamp()+120;
            Color red = new Color() { R = 255, A = 50 };
            Color green = new Color() { G = 255, A = 50 };

            if (textbox_Password.Password != textbox_Password_Retype.Password
                || textbox_Password.Password.Length < 5 || textbox_Password.Password.Length > 16)
            {
                textblock_Info.Text = (textbox_Password.Password.Length < 5 || textbox_Password.Password.Length > 16)? "Password must have 5-16 chars;" : "You just typed two diffrent passwords.";
                textbox_Password.Background = new SolidColorBrush(red);
                textbox_Password_Retype.Background = new SolidColorBrush(red);
                textblock_Info.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            else if(!IsValidEmail(textbox_Email.Text))
            {
                textblock_Info.Text = "Please type valid e-mail adress";
                textbox_Email.Background = new SolidColorBrush(red);
                return;
            }
            
            button_Register.IsEnabled = false;

            Model.SQLManager.RegisterAccountStatus result = await Model.SQLManager.RegisterNewUser(textbox_Login.Text, textbox_Password.Password, textbox_Email.Text);

            switch (result)
            {
                case Model.SQLManager.RegisterAccountStatus.Email_Alerady_Exists:
                    textblock_Info.Text = "E-mail is in use.";
                    textbox_Email.Background = new SolidColorBrush(red);
                    break;

                case Model.SQLManager.RegisterAccountStatus.Login_Alerady_Exists:
                    textblock_Info.Text = "Login is in use. You think, that someone stolen you this nickname? Write to grs4_98@o2.pl";
                    textblock_Info.Foreground = new SolidColorBrush(Colors.Red);
                    textbox_Login.Background = new SolidColorBrush(red);
                    break;

                case Model.SQLManager.RegisterAccountStatus.Succesful:
                    textblock_Info.Text = "Your account has been created succesfully.";
                    button_Register.IsEnabled = false;
                    textblock_Info.Foreground = new SolidColorBrush(Colors.Green);
                    textbox_Email.Background = new SolidColorBrush(green);
                    textbox_Login.Background = new SolidColorBrush(green);
                    textbox_Password.Background = new SolidColorBrush(green);
                    textbox_Password_Retype.Background = new SolidColorBrush(green);

                    MessageBox.Show("You just created a new account." + Environment.NewLine + Environment.NewLine +
                        "Login: " + textbox_Login.Text + Environment.NewLine +
                        "E-mail: " + textbox_Email.Text + Environment.NewLine + Environment.NewLine +
                        "(eng)Before login, your account must be validated. Write to grs4_98@o2.pl, or on Gadu-Gadu: 38862128 for it." + Environment.NewLine + Environment.NewLine +
                        "Send me your login, or email address showed above." + Environment.NewLine +
                        "I'm checking new requests very often, so it's nearly possible, that your account will be available today.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    await Task.Delay(TimeSpan.FromSeconds(2));
                    LoginWindow.gui.ShowMainView();
                    break;

                case Model.SQLManager.RegisterAccountStatus.Failed:
                    textblock_Info.Text = "There's problem with register a new account. Try again later";
                    textblock_Info.Foreground = new SolidColorBrush(Colors.Red);
                    break;
            }

            button_Register.IsEnabled = true;
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow.gui.ShowMainView();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                return new System.Net.Mail.MailAddress(email).Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
