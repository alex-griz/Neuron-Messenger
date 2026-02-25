using System.Collections.ObjectModel;
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
        private void MakeAdmin(object sender, RoutedEventArgs e)
        {
            
        }
        private void DeleteMember(object sender, RoutedEventArgs e)
        {
            
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
