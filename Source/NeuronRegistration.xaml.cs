using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
    public partial class NeuronRegistration : Window
    {
        public NeuronRegistration()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Name = NameBox.Text;
            string Username = UsernameBox.Text;
            string Password = PasswordBox.Password;

            DataBase DB = new DataBase();
            MySqlCommand command = new MySqlCommand("INSERT INTO `authbase` (`Username`, `Name`, `Password`) VALUES (@Username , @Name, @Password)",
            DB.getConnection());
            command.Parameters.Add("@Username", MySqlDbType.VarChar).Value = Username;
            command.Parameters.Add("@Name", MySqlDbType.VarChar).Value = Name;
            command.Parameters.Add("@Password", MySqlDbType.VarChar).Value = Password;

            command.ExecuteNonQuery();
            MessageBox.Show("Успешная регистрация!", "Neuron - регистрация", MessageBoxButton.OKCancel, MessageBoxImage.Information);
        }
    }
}
