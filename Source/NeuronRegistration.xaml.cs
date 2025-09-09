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
using System.Data;

namespace Neuron
{
    public partial class NeuronRegistration : Window
    {
        public string Username;
        DataBase DB = new DataBase();
        public NeuronRegistration()
        {
            InitializeComponent();
        }
        
        public void Button_Click(object sender, RoutedEventArgs e)
        {
            string Name = NameBox.Text;
            Username = UsernameBox.Text;
            string Password = PasswordBox.Password;

            using(var connection = DB.GetNewConnection())
            {
                using (var command = new MySqlCommand("INSERT INTO `authbase` (`Username`, `Name`, `Password`) VALUES (@Username , @Name, @Password)",
                    connection))
                {
                    command.Parameters.Add("@Username", MySqlDbType.VarChar).Value = Username;
                    command.Parameters.Add("@Name", MySqlDbType.VarChar).Value = Name;
                    command.Parameters.Add("@Password", MySqlDbType.VarChar).Value = Password;

                    if (Check())
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        MessageBox.Show("Успешная регистрация!", "Neuron - регистрация", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Такое имя пользователя уже существует! Придумайте другое", "Neuron - регистрация", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    }
                }
            }
   
             
        }
        private bool Check()
        {
            bool result = false;
            DataTable AuthResult = new DataTable();

            using (var connection = DB.GetNewConnection())
            {
                using (var command = new MySqlCommand("SELECT `Name` FROM `authbase` WHERE `Username` = @Username",
                  connection))
                {
                    command.Parameters.Add("@Username", MySqlDbType.VarChar).Value = Username;
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(AuthResult);
                    }
                }
            }
            
            if (AuthResult.Rows.Count == 0)
            {
                result = true;
            }
            return result;
        }
    }
}