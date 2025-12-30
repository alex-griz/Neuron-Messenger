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
using System.Text.Json;
using System.IO;

namespace Neuron
{
    public partial class MainWindow : Window
    {
        public static string Login;
        public static string Name;
        public MainWindow()
        {
            InitializeComponent();

            SaveLoginData loginData = ReadLoginData();
            if (loginData.Login != null)
            {
                Login = loginData.Login;
                LoginFunction(loginData.Password);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login = LoginBox.Text;
            string Password = PasswordBox.Password;

            LoginFunction(Password);
        }
        private void LoginFunction(string Password)
        {
            DataBase DB = new DataBase();
            using DataTable AuthResult = new DataTable();
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
                Name = AuthResult.Rows[0][1].ToString();
                if (SaveLogin.IsChecked == true)
                {
                    var loginData = new SaveLoginData { Login = Login, Password = Password };
                    WriteLogin(loginData);
                }

                NeuronMain window = new NeuronMain();
                window.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Неверное имя пользователя или пароль, попробуйте ещё!", "Neuron - Авторизация", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            NeuronRegistration window = new NeuronRegistration();
            window.Show();
        }
        private void WriteLogin(SaveLoginData loginData)
        {
            string json = JsonSerializer.Serialize(loginData);
            File.WriteAllText("SavedLoginData.json", json);
        }
        private SaveLoginData ReadLoginData()
        {
            string json = File.ReadAllText("SavedLoginData.json");
            try
            {
                return JsonSerializer.Deserialize<SaveLoginData>(json);
            }
            catch
            {
                SaveLoginData saveLoginData = new SaveLoginData { Login = null, Password= null};
                return saveLoginData;
            }
            
        }
    }
    public class SaveLoginData()
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}