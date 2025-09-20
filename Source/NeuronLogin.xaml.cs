using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Data;

namespace Neuron
{
    public partial class MainWindow : Window
    {
        public int ChatCounter;
        public static string Login;
        public static string Name;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login = LoginBox.Text;
            string Password = PasswordBox.Password;
            DataBase DB = new DataBase();
            DataTable AuthResult = new DataTable();

            using (var connection = DB.GetNewConnection())
            {
                using (var command = new MySqlCommand("SELECT `Username`, `Name` FROM `authbase` WHERE `Username` = @login AND `Password` = @password",
            connection))
                {
                    command.Parameters.Add("@login", MySqlDbType.VarChar).Value = Login;
                    command.Parameters.Add("@password", MySqlDbType.VarChar).Value = Password;
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        connection.Open();
                        adapter.Fill(AuthResult);
                    }
                }
            }
            if (AuthResult.Rows.Count > 0)
            {
                MessageBox.Show("Успешный вход!", "Neuron - Авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                Name = AuthResult.Rows[0][1].ToString();

                NeuronMain window = new NeuronMain();
                window.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Неверное имя пользователя или пароль, попробуйте ещё!", "Neuron - Авторизация", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            NeuronRegistration window = new NeuronRegistration();
            window.Show();
        }
    }
}