using MySql.Data.MySqlClient;
using Mysqlx.Crud;
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
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `authbase` WHERE `Username` = @UN");

            command.Parameters.Add("@UN", MySqlDbType.VarChar).Value = Username;
            adapter.SelectCommand = command;
            try
            {
                db.OpenConnection();
                adapter.Fill(dataTable);
                db.CloseConnection();

                command = new MySqlCommand("INSERT INTO `contactbase` (`Owner`, `ContactUserName`, `ContactName`) " +
                "VALUES (@Owner , @ContactuserName , @ContactName)", db.getConnection());
                command.Parameters.Add("@Owner", MySqlDbType.VarChar).Value = MainWindow.Login;
                command.Parameters.Add("@ContactUserName", MySqlDbType.VarChar).Value= Username;
                command.Parameters.Add("@ContactName", MySqlDbType.VarChar).Value = dataTable.Rows[0][1];

                db.OpenConnection();
                command.ExecuteNonQuery();
                db.CloseConnection();

                MessageBox.Show("Контакт успешно добавлен!", "Новый контакт", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                this.Close();
            }
            catch
            {
                MessageBox.Show("Такого пользователя не существует!", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }

        }
    }
}
