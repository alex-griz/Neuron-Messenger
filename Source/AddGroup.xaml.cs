using System.Windows;

namespace Neuron
{
    public partial class AddGroup : Window
    {
        public AddGroup()
        {
            InitializeComponent();
        }

        private async void AddGroupClick(object sender, RoutedEventArgs e)
        {
            string chat_name = ChatNameBox.Text;

            var response = await MainWindow.client.PostAsync($"http://localhost:5156/AddGroup?name={chat_name}", null);
            var result = int.Parse(await response.Content.ReadAsStringAsync());
            if(result == 1)
            {
                this.Close();
                MessageBox.Show("Группа успешно создана!", "Новая группа", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ошибка на стороне сервера", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
        }
    }
}
