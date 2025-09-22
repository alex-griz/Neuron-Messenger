using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

                using (var command = new MySqlCommand("INSERT INTO `contactbase` (`ChatID`, `Member`, `ChatName`) "+
                "VALUES (@CI , @ME, @CN)", connection))
                {
                    try
                    {
                        command.Parameters.Add("@CI", MySqlDbType.VarChar).Value = 
                           Convert.ToInt32(dataTable.Rows[dataTable.Rows.Count - 1][0])+1;
                        command.Parameters.Add("@ME", MySqlDbType.VarChar).Value = MainWindow.Login;
                        command.Parameters.Add("@CN", MySqlDbType.VarChar).Value = ChatName;

                        connection.Open();
                        command.ExecuteNonQuery();
                        command.Parameters.Remove(MainWindow.Login);

                        command.Parameters.Add("@ME", MySqlDbType.VarChar).Value = Username;

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
}
