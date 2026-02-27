using System.Windows;
using System.Windows.Controls;

namespace Neuron
{
    public partial class NeuronRegistration : Window
    {
        public string Username;
        public NeuronRegistration()
        {
            InitializeComponent();
        }

        public async void Button_Click(object sender, RoutedEventArgs e)
        {
            string Name = NameBox.Text;
            Username = UsernameBox.Text;
            string Password = PasswordBox.Password;

            var response = await MainWindow.client.PostAsync($"http://localhost:5156/Reg?username={Username}&name={Name}&password={Password}", null);
            var result = int.Parse(await response.Content.ReadAsStringAsync());
            switch (result)
            {
                case 0:
                    MessageBox.Show("Возникла ошибка на стороне сервера", "Ошибка регистрации", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    break;
                case 1:
                    MessageBox.Show("Успешная регистрация!", "Neuron - регистрация", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    break;
                case 2:
                    MessageBox.Show("Такое имя пользователя уже существует! Пожалуйста, выберите другое", "Neuron - регистрация", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    break;
            }
        }
    }
}