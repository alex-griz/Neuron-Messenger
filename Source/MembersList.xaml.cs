using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Neuron
{
    public partial class MembersList : Window
    {
        DataBase db = new DataBase();
        public static ObservableCollection<CheckItem> users = new ObservableCollection<CheckItem>();
        public MembersList()
        {
            InitializeComponent();
            NameBox.Text = NeuronMain.ChooseChatName;
            LoadContacts();
        }
        private async void AddMember(object sender, RoutedEventArgs e)
        {
            AddMember window = new AddMember();
            window.Show();
        }
        private async void MakeAdmin(object sender, RoutedEventArgs e)
        {
            var selectedUsers = users.Where(u => u.IsSelected).Select(u => u.Name).ToArray();
            var json = JsonSerializer.Serialize(selectedUsers);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await MainWindow.client.PostAsync( $"http://localhost:5156/MakeAdmin?ChatId={NeuronMain.ChooseContact}",content);
            var result = int.Parse(await response.Content.ReadAsStringAsync());

            switch (result)
            {
                case 0:
                    MessageBox.Show("Возникла ошибка на стороне сервера", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    break;
                case 1:
                    MessageBox.Show("Изменения успешно применены", "Neuron", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    break;
                case 2:
                    MessageBox.Show("У вас недостаточно прав в этой группе", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    break;
            }
        }
        private async void DeleteMember(object sender, RoutedEventArgs e)
        {
            var selectedUsers = users.Where(u => u.IsSelected).Select(u => u.Name).ToArray();
            var json = JsonSerializer.Serialize(selectedUsers);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await MainWindow.client.PostAsync($"http://localhost:5156/DeleteMember?ChatId={NeuronMain.ChooseContact}", content);
            var result = int.Parse(await response.Content.ReadAsStringAsync());

            switch (result)
            {
                case 0:
                    MessageBox.Show("Возникла ошибка на стороне сервера", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    break;
                case 1:
                    MessageBox.Show("Изменения успешно применены", "Neuron", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    break;
                case 2:
                    MessageBox.Show("У вас недостаточно прав в этой группе", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    break;
            }
        }
        private void LoadContacts()
        {
            MembersListBox.ItemsSource = users;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string NewChatName = NameBox.Text;
        }
    }
    public class CheckItem()
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
