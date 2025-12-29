using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;

namespace Neuron
{
    public partial class ProfileView : Window
    {
        public static string username = "";
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

            command.Parameters.AddWithValue("@UN", username);

            connection.Open();
            adapter.Fill(profileTable);

            UsernameBox.Text = username;
            NameBox.Text = profileTable.Rows[0][2].ToString();
            BioBox.Text = profileTable.Rows[0][3].ToString();

            if (username != MainWindow.Login)
            {
                UsernameBox.IsReadOnly = true;
                NameBox.IsReadOnly = true;
                BioBox.IsReadOnly = true;
                SaveButton.Visibility = Visibility.Hidden;
            }
        }
        private void SaveProfile(object sender, RoutedEventArgs e)
        {
            DataBase db = new DataBase();
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand(SQL_Injections.SaveProfile, connection);

            command.Parameters.AddWithValue("@UN", UsernameBox.Text);
            command.Parameters.AddWithValue("@U", NameBox.Text);
            command.Parameters.AddWithValue("@D", BioBox.Text);
            command.Parameters.AddWithValue("@UI", null);
            try
            {
                connection.Open();
                command.ExecuteNonQuery();

                //command.CommandText = SQL_Injections.SaveLoginData;
                //command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Не удалось сохранить изменения","Ошибка отправки", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
        }
    }
}