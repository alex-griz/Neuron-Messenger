using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;

namespace Neuron
{
    public partial class ProfileView : Window
    {
        public static string target_username = "";
        public ProfileView()
        {
            InitializeComponent();

            LoadData();
        }
        private void LoadData()
        {
            DataBase db = new DataBase();
            DataTable profileTable = new DataTable();
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand(SQL_Injections.LoadProfile, connection);
            using var adapter = new MySqlDataAdapter(command);

            command.Parameters.AddWithValue("@UN", target_username);

            connection.Open();
            adapter.Fill(profileTable);

            UsernameBox.Text = target_username;
            NameBox.Text = profileTable.Rows[0][1].ToString();
            BioBox.Text = profileTable.Rows[0][2].ToString();

            if (target_username != MainWindow.Login)
            {
                UsernameBox.IsReadOnly = true;
                NameBox.IsReadOnly = true;
                BioBox.IsReadOnly = true;
                SaveButton.Visibility = Visibility.Hidden;
            }
        }
        private async void SaveProfile(object sender, RoutedEventArgs e)
        {
            string new_username = UsernameBox.Text;
            string new_name = NameBox.Text;
            string new_bio = BioBox.Text;

            var response = await MainWindow.client.PostAsync($"http://localhost:5156/ChangeProfileData?username={new_username}&name={new_name}&bio={new_bio}", null);
            var result = int.Parse(await response.Content.ReadAsStringAsync());
            if (result == 0)
            {
                MessageBox.Show("Возникла ошибка на стороне сервера", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
        }
    }
}