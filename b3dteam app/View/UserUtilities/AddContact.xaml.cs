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
using System.Windows.Shapes;

namespace b3dteam_app.View.UserUtilities
{
    /// <summary>
    /// Interaction logic for AddContact.xaml
    /// </summary>
    public partial class AddContact : Window
    {
        public AddContact()
        {
            InitializeComponent();
        }

        private void textbox_FindBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private async void button_Add_Click(object sender, RoutedEventArgs e)
        {
            var clickedItem = listview_Users.SelectedItem as TextBlock;

            if(clickedItem == null ||
                clickedItem.Text == "No users were found")
            {
                return;
            }
            else
            {
                SetButtonsStatus(false);
                var user = await new ChatManager.Chat(helper.SQLManager.SqlConnection).GetUser(int.Parse(clickedItem.Text.Split('#')[0]));

                if(user == null)
                {
                    SetButtonsStatus(true);
                    MessageBox.Show("Error occured");
                    return;
                }
                else
                {
                    SetButtonsStatus(true);
                    MessageBox.Show("User added to your list");
                    //
                }
            }

        }

        private async void button_Find_Click(object sender, RoutedEventArgs e)
        {
            listview_Users.Items.Clear();

            SetButtonsStatus(false);
            int res = -1;
            if (int.TryParse(textbox_FindBox.Text, out res))
            {
                var user = (await helper.SQLManager.GetUser(res));

                if(user == null)
                {
                    SetButtonsStatus(true);
                    AddNoItemsFoundText();
                    return;
                }
                listview_Users.Items.Add(new TextBlock() { Text = $"{user.userid}# {user.login}" });
            }
            else
            {
                (await helper.SQLManager.GetUsers()).Where(p => (p.login.ToLower().StartsWith(textbox_FindBox.Text.ToLower()))).ToList().ForEach(
                    p => listview_Users.Items.Add(new TextBlock() { Text = $"{p.userid}# {p.login}" }));
            }

            if(listview_Users.Items.Count == 0)
            {
                AddNoItemsFoundText();
            }
            SetButtonsStatus(true);
        }

        private void AddNoItemsFoundText()
        {
            listview_Users.Items.Add(new TextBlock() { Text = "No users were found" });
        }
        private void SetButtonsStatus(bool enabled)
        {
            button_Add.IsEnabled = enabled;
            button_Find.IsEnabled = enabled;
        }
    }
}
