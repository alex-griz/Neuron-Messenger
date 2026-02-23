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

        }
        private void MakeAdmin(object sender, RoutedEventArgs e)
        {
            
        }
        private void DeleteMember(object sender, RoutedEventArgs e)
        {
            
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
