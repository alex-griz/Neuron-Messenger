using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;

namespace Neuron
{
    public partial class AddGroup : Window
    {
        public AddGroup()
        {
            InitializeComponent();
        }

        private void AddGroupClick(object sender, RoutedEventArgs e)
        {
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

                using (var command = new MySqlCommand("INSERT INTO `contactbase` (`ChatID`, `Member`, `Role`) " +
                "VALUES (@CI , @ME, @R)", connection))
                {
                    try
                    {
                        command.Parameters.Add("@CI", MySqlDbType.VarChar).Value =
                           Convert.ToInt32(dataTable.Rows[dataTable.Rows.Count - 1][0]) + 1;
                        command.Parameters.Add("@ME", MySqlDbType.VarChar).Value = MainWindow.Login;
                        command.Parameters.Add("@R", MySqlDbType.Int16).Value = 1;

                        connection.Open();
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT INTO `ChatBase` (`ChatID`, `ChatName`, `Description`, `Photo`, `Type`) VALUES (@CI, @CN, NULL, NULL, 1)";
                        command.Parameters.AddWithValue("@CN", ChatName);
                        command.ExecuteNonQuery();

                        this.Close();
                        MessageBox.Show("Группа успешно создана!", "Новая группа", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка при создании группы!", "Новая группа", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
