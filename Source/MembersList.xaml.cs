using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Windows;

namespace Neuron
{
    public partial class MembersList : Window
    {
        DataBase db = new DataBase();
        public static ObservableCollection<CheckItem> users = new ObservableCollection<CheckItem>();
        public MembersList()
        {
            InitializeComponent();
            NameBox.Text = NeuronMain.ChooseChatName;
            LoadContacts();
        }
        private void AddMember(object sender, RoutedEventArgs e)
        {
            /*if (NeuronMain.clicked.IsAdmin == 0 || NeuronMain.clicked.Type == 0)
            {
                MessageBox.Show("Недостаточно прав", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                AddMember addMember = new AddMember();
                addMember.Show();
            }*/
        }
        private void MakeAdmin(object sender, RoutedEventArgs e)
        {
            /*if (NeuronMain.clicked.IsAdmin == 0 || NeuronMain.clicked.Type == 0)
            {
                MessageBox.Show("Недостаточно прав", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                var SelectedUsers = users.Where(u => u.IsSelected).Select(u => u.Name).ToArray();
                using (var connection = db.GetNewConnection())
                {
                    using (var command = new MySqlCommand("UPDATE `contactbase` SET `Role` = 1 WHERE `ChatID` = @CI AND `Member` = @ME", connection))
                    {
                        command.Parameters.AddWithValue("@CI", NeuronMain.ChooseContact);
                        command.Parameters.Add("@ME", MySqlDbType.VarChar);
                        connection.Open();
                        foreach(string username  in SelectedUsers)
                        {
                            command.Parameters["@ME"].Value = username; 
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }*/
        }
        private void DeleteMember(object sender, RoutedEventArgs e)
        {
            /*if (NeuronMain.clicked.IsAdmin == 0 || NeuronMain.clicked.Type == 0)
            {
                MessageBox.Show("Недостаточно прав", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                var SelectedUsers = users.Where(u => u.IsSelected).Select(u => u.Name).ToArray();
                using (var connection = db.GetNewConnection())
                {
                    using (var command = new MySqlCommand("DELETE FROM `contactbase` WHERE `ChatID` = @CI AND `Member` = @ME", connection))
                    {
                        command.Parameters.AddWithValue("@CI", NeuronMain.ChooseContact);
                        command.Parameters.Add("@ME", MySqlDbType.VarChar);
                        connection.Open();

                        foreach (string username in SelectedUsers)
                        {
                            command.Parameters["@ME"].Value = username;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }*/
        }
        private void LoadContacts()
        {
            MembersListBox.ItemsSource = users;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string NewChatName = NameBox.Text;
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand("UPDATE `ChatBase` SET `ChatName` = @CN WHERE `ChatID` = @CI", connection);
  
            command.Parameters.AddWithValue("@CN", NewChatName);
            command.Parameters.AddWithValue("@CI", NeuronMain.ChooseContact.ToString());
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Не удалось переименовать чат", "Ошибка отправки", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    public class CheckItem()
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
