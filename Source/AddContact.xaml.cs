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
            DataBase db = new DataBase();
            DataTable dataTable = new DataTable();
            using (var connection = db.GetNewConnection())
            {
                using (var command = new MySqlCommand("SELECT * FROM `authbase` WHERE `Username` = @UN", connection))
                {
                    command.Parameters.Add("@UN", MySqlDbType.VarChar).Value = Username;
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
                using (var command = new MySqlCommand("INSERT INTO `contactbase` (`Owner`, `ContactUserName`, `ContactName`) "+
                "VALUES (@Owner , @ContactuserName , @ContactName)", connection))
                {
                    command.Parameters.Add("@Owner", MySqlDbType.VarChar).Value = MainWindow.Login;
                    command.Parameters.Add("@ContactUserName", MySqlDbType.VarChar).Value = Username;
                    command.Parameters.Add("@ContactName", MySqlDbType.VarChar).Value = dataTable.Rows[0][1];
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        MessageBox.Show("Контакт успешно добавлен!", "Новый контакт", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
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
