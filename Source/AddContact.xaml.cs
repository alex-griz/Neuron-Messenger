using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;

namespace Neuron
{
    public partial class AddContact : Window
    {
        public AddContact()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Username = ContactUsername.Text;
            string ChatName = ChatNameBox.Text;
            DataBase db = new DataBase();
            DataTable dataTable = new DataTable();
            using (var connection = db.GetNewConnection())
            {
                using (var command = new MySqlCommand("SELECT `ChatID` FROM `contactbase`", connection))
                {
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
                dataTable.DefaultView.Sort = "ChatID ASC";
                DataView dataView = dataTable.DefaultView;
                dataTable = dataView.ToTable();

                try
                {
                    using var command = new MySqlCommand("INSERT INTO `contactbase` (`ChatID`, `Member`,`SecondMember`, `Role`) VALUES (@CI , @ME,@SM, @R)", connection);
                    command.Parameters.Add("@CI", MySqlDbType.Int16).Value =
                           Convert.ToInt32(dataTable.Rows[dataTable.Rows.Count - 1][0]) + 1;
                    command.Parameters.Add("@ME", MySqlDbType.VarChar).Value = MainWindow.Login;
                    command.Parameters.Add("@SM", MySqlDbType.VarChar).Value = Username;
                    command.Parameters.Add("@R", MySqlDbType.Int16).Value = 1;

                    connection.Open();
                    command.ExecuteNonQuery();

                    command.Parameters["@ME"].Value = Username;
                    command.Parameters["@SM"].Value = MainWindow.Login;
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO `ChatBase` (`ChatID`, `ChatName`, `Description`, `Photo` , `Type`) VALUES (@CI, NULL, NULL, NULL, 0)";
                    command.ExecuteNonQuery();

                    this.Close();
                    MessageBox.Show("Контакт успешно добавлен!", "Новый контакт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show("Такого пользователя не существует!", "Новый контакт", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                }
            }
        }
    }
}
