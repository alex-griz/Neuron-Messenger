using System.Windows;

namespace Neuron
{
    public partial class AddMember : Window
    {
        public AddMember()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = MemberUsername.Text;
            var response = await MainWindow.client.PostAsync($"http://localhost:5156/AddMember?ChatId={NeuronMain.ChooseContact}&target_member={username}", null);
            var result = int.Parse(await response.Content.ReadAsStringAsync());
            switch (result)
            {
                case 0:
                    MessageBox.Show("Возникла ошибка на стороне сервера", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    break;
                case 1:
                    MessageBox.Show("Пользователь успешно добавлен", "Neuron", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    break;
                case 2:
                    MessageBox.Show("У вас недостаточно прав в этой группе", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    break;
            }
            this.Close();
        }
    }
}
