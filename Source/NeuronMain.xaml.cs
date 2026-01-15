using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

namespace Neuron
{
    public partial class NeuronMain : Window
    {
        public static int ChooseContact;
        public static string ChooseChatName;
        public static ContactButton clicked = null;
        private static HubConnection hubConnection;

        DataBase db = new DataBase();

        Commands commands = new Commands();

        public NeuronMain()
        {
            InitializeComponent();

            hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5156/chatHub?username="+ MainWindow.Login)
                .WithAutomaticReconnect()
                .Build();

            hubConnection.On<ChatMessage>("GetMessage", (message) =>
            {
                if (message.ChatID == ChooseContact)
                {
                    commands.UpdateMessages(this, message);
                }
            });
            hubConnection.StartAsync();

            commands.LoadContacts(this, ChatList);
        }
        private void LogOut(object sender, RoutedEventArgs e)
        {
            MainWindow.Login = "";
            string json = "";
            File.WriteAllText("SavedLoginData.json", json);

            this.Close();
            MainWindow window = new MainWindow();
            window.Show();
        }
        private async void SelectContact(object sender, RoutedEventArgs e)
        {
            var clickedButton = (Button)sender;

            string selectContactName = clickedButton.Content.ToString();
            clicked = clickedButton.DataContext as ContactButton;
            ChooseContact = Convert.ToInt32(clickedButton.Tag);
            ChooseChatName = selectContactName;
            HeadNameLabel.Content = selectContactName;

            commands.LoadMessages(MessagesField, MessageField, SendButton);
        }

        private void Add_Contact(object sender, RoutedEventArgs e)
        {
            AddContact window = new AddContact();
            window.Show();
        }
        private void Add_Group(object sender, RoutedEventArgs e)
        {
            AddGroup window = new AddGroup();
            window.Show();
        }
        private void SendMessage(object sender, RoutedEventArgs e)
        {
            commands.SendMessage(MessageField.Text, hubConnection);
            MessageField.Clear();
        }
        private void OpenChatFeatures(object sender, RoutedEventArgs e)
        {
            if (NeuronMain.clicked.Type == 0)
            {
                OpenProfileView();
            }
            else
            {
                MembersList window = new MembersList();
                window.Show();
            }
        }
        private void OpenProfileEditor(object sender, RoutedEventArgs e)
        {
            ProfileView.username = MainWindow.Login;
            ProfileView window = new ProfileView();
            window.Show();
        }
        private void OpenProfileView()
        {
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand(SQL_Injections.LoadMembers, connection);
            command.Parameters.AddWithValue("@CI", ChooseContact.ToString());
            using var adapter = new MySqlDataAdapter(command);
            using DataTable members = new DataTable();

            connection.Open();
            adapter.Fill(members);

            if(members.Rows[0][0] != MainWindow.Login)
            {
                ProfileView.username = members.Rows[0][0].ToString();
            }
            else
            {
                ProfileView.username = members.Rows[1][0].ToString();
            }
            ProfileView window = new ProfileView();
            window.Show();
        }
        private void DeleteChat(object sender, RoutedEventArgs e)
        {
            if (clicked.IsAdmin == 0)
            {
                var answer = MessageBox.Show("Вы действительно хотите покинуть этот чат? Вы снова сможете присоединиться к ней", "Подтверждение действия", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer == MessageBoxResult.Yes)
                {
                    using var connection = db.GetNewConnection();
                    using var command = new MySqlCommand(SQL_Injections.LeaveGroup, connection);
                    command.Parameters.AddWithValue("@ME", MainWindow.Login);
                    command.Parameters.AddWithValue("@CI", ChooseContact.ToString());

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            else
            {
                var answer = MessageBox.Show("Вы действительно хотите безвозвратно удалить этот чат?", "Подтверждение действия", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer == MessageBoxResult.Yes)
                {
                    using var connection = db.GetNewConnection();
                    using var command = new MySqlCommand(SQL_Injections.DeleteGroup, connection);
                    command.Parameters.AddWithValue("@CI", ChooseContact.ToString());

                    connection.Open();
                    command.ExecuteNonQuery();

                    command.CommandText = SQL_Injections.DeleteGroupMessages;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
    public class Commands()
    {
        private DataBase db = new DataBase();
        private string CurrentDate;
        public async Task LoadMessages(ListBox MessagesField, TextBox messageField, Button sendButton)
        {
            using DataTable MessageList = new DataTable();

            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand(SQL_Injections.GetMessages, connection);
            using var adapter = new MySqlDataAdapter(command);

            command.Parameters.Add("@CI", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact;
            connection.Open();
            adapter.Fill(MessageList);

            MessagesField.Items.Clear();
            if (MessageList.Rows.Count != 0)
            {
                CurrentDate = MessageList.Rows[0][4].ToString();
                MessagesField.Items.Add(CurrentDate);
                for (int i = 0; i < MessageList.Rows.Count; i++)
                {
                    if (MessageList.Rows[i][4].ToString() == CurrentDate)
                    {
                        MessagesField.Items.Add(MessageList.Rows[i][1] + "\n \n" + MessageList.Rows[i][2] + "\n \n" + MessageList.Rows[i][3].ToString());
                    }
                    else
                    {
                        CurrentDate = MessageList.Rows[i][4].ToString();
                        MessagesField.Items.Add(CurrentDate);
                        MessagesField.Items.Add(MessageList.Rows[i][1] + "\n \n" + MessageList.Rows[i][2] + "\n \n" + MessageList.Rows[i][3].ToString());
                    }
                }
            }
        }
        public void UpdateMessages(NeuronMain neuronMain, ChatMessage chatMessage)
        {
            if (chatMessage.Date != CurrentDate)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    {
                        neuronMain.MessagesField.Items.Add(chatMessage.Date);
                    });
                CurrentDate = chatMessage.Date;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                neuronMain.MessagesField.Items.Add(
                    $"{chatMessage.Sender}\n\n{chatMessage.Message}\n\n{chatMessage.Time}"
                );
            });
        }
        public async void SendMessage(string MessageText, HubConnection hubConnection)
        {
            ChatMessage message = new ChatMessage();
            message.ChatID = NeuronMain.ChooseContact;
            message.Sender = MainWindow.Name;
            message.Message = MessageText;
            message.Time = DateTime.Now.ToString("HH:mm");
            message.Date = DateTime.Now.ToString("dd.MM.yyyy");

            await hubConnection.InvokeAsync("SendMessage", NeuronMain.ChooseContact, message);

            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand(SQL_Injections.SendMessage, connection);

            connection.Open();

            command.Parameters.Add("@S", MySqlDbType.VarChar).Value = message.Sender;
            command.Parameters.Add("@T", MySqlDbType.VarChar).Value = message.Time;
            command.Parameters.Add("@M", MySqlDbType.Text).Value = message.Message;
            command.Parameters.Add("@CI", MySqlDbType.Int32).Value = message.ChatID;
            command.Parameters.Add("@D", MySqlDbType.VarChar).Value = message.Date;

            try
            {
                command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Не удалось отправить сообщение", "Ошибка отправки", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public async void LoadContacts(NeuronMain neuronMain, ListBox chatListBox)
        {
            using DataTable ContactsList = new DataTable();
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand(SQL_Injections.GetContacts, connection);
            using var adapter = new MySqlDataAdapter(command);

            command.Parameters.Add("@Username", MySqlDbType.VarChar).Value = MainWindow.Login;

            connection.Open();
            adapter.Fill(ContactsList);

            List<int> chat_list = new List<int>();

            for (int i = 0; i < ContactsList.Rows.Count; i++)
            {
                var contact = new ContactButton
                {
                    ButtonName = ContactsList.Rows[i][2].ToString(),
                    ChatID = Convert.ToInt32(ContactsList.Rows[i][0]),
                    Type = Convert.ToInt16(ContactsList.Rows[i][3]),
                    IsAdmin = Convert.ToInt16(ContactsList.Rows[i][4])
                };
                chatListBox.Items.Add(contact);
            }
        }
        public void UpdateContacts()
        {

        }
    }
    public class ContactButton()
    {
        public string ButtonName { get; set; }
        public int ChatID { get; set; }
        public int Type { get; set; }
        public int IsAdmin { get; set; }
    }
    public class ChatMessage
    {
        public int ChatID { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
    }
}