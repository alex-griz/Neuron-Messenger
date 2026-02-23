using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Neuron
{
    public partial class MainWindow : Window
    {
        public HttpClient client = new HttpClient();
        public static string Login;
        public static string Jwt_Security_Token = "";
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
        private async void LoginFunction(string Password)
        {
            var response = await client.GetAsync($"http://localhost:5156/Login?username={Login}&password={Password}");
            var result = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<LoginResult>(result);

            switch (data.status)
            {
                case 0:
                    MessageBox.Show("Возникла ошибка на сервере авторизации. Повторите попытку позже.", "Ошибка авторизации", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    break;
                case 1:
                    Jwt_Security_Token = data.token;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", data.token);
                    if (SaveLogin.IsChecked == true)
                    {
                        var loginData = new SaveLoginData { Login = Login, Password = Password };
                        WriteLogin(loginData);
                    }
                    NeuronMain window = new NeuronMain();
                    window.Show();
                    this.Hide();
                    break;
                case 2:
                    MessageBox.Show("Неверное имя пользователя или пароль", "Ошибка авторизации", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    break;
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