using System.Windows;

namespace Neuron
{
    public partial class AddContact : Window
    {
        public AddContact()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = ContactUsername.Text;

            var response = await MainWindow.client.PostAsync($"http://localhost:5156/AddContact?target_username={username}", null);
            var result = int.Parse(await response.Content.ReadAsStringAsync());
            switch (result)
            {
                case 0:
                    MessageBox.Show("Ошибка на стороне сервера", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 1:
                    this.Close();
                    break;
            }
        }
    }
}
