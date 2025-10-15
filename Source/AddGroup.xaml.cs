using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

                using (var command = new MySqlCommand("INSERT INTO `contactbase` (`ChatID`, `Member`, `ChatName`, `IsGroup`, `IsAdmin`) " +
                "VALUES (@CI , @ME, @CN, @IG, @IA)", connection))
                {
                    try
                    {
                        command.Parameters.Add("@CI", MySqlDbType.VarChar).Value =
                           Convert.ToInt32(dataTable.Rows[dataTable.Rows.Count - 1][0]) + 1;
                        command.Parameters.Add("@ME", MySqlDbType.VarChar).Value = MainWindow.Login;
                        command.Parameters.Add("@CN", MySqlDbType.VarChar).Value = ChatName;
                        command.Parameters.Add("@IG", MySqlDbType.Int16).Value = 1;
                        command.Parameters.Add("@IA", MySqlDbType.Int16).Value = 1;

                        connection.Open();
                        command.ExecuteNonQuery();

                        this.Close();
                        MessageBox.Show("Группа успешно создана!", "Новая группа", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка при создании группы!!", "Новая группа", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
