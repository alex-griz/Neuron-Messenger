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
            commands.LoadContacts(this, ChatList);

            hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5156/chatHub")
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
        }
        private void LogOut(object sender, RoutedEventArgs e)
        {
            MainWindow.Login = null;
            string json = null;
            File.WriteAllText("SavedLoginData.json", json);

            this.Close();
            MainWindow window = new MainWindow();
            window.Show();
        }
        private async void SelectContact(object sender, RoutedEventArgs e)
        {
            await hubConnection.InvokeAsync("LeaveChat", ChooseContact.ToString());

            var clickedButton = (Button)sender;

            string selectContactName = clickedButton.Content.ToString();
            clicked = clickedButton.DataContext as ContactButton;
            ChooseContact = Convert.ToInt32(clickedButton.Tag);
            ChooseChatName = selectContactName;
            HeadNameLabel.Content = selectContactName;

            commands.LoadMessages(MessagesField, MessageField, SendButton);
            await hubConnection.InvokeAsync("JoinChat", ChooseContact.ToString());
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
                //открытие просмотра профиля пользователя
            }
            else
            {
                MembersList window = new MembersList();
                window.Show();
            }
        }
        private void DeleteChat(object sender, RoutedEventArgs e)
        {
            if (clicked.IsAdmin == 0)
            {
                var answer = MessageBox.Show("Вы действительно хотите покинуть этот чат? Вы снова сможете присоединиться к ней", "Подтверждение действия", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer == MessageBoxResult.Yes)
                {
                    using var connection = db.GetNewConnection();
                    using var command = new MySqlCommand("DELETE FROM `ContactBase` WHERE `Member` = @ME AND `ChatID` = @CI", connection);
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
                    using var command = new MySqlCommand("DELETE FROM `ContactBase` WHERE `ChatID` = @CI", connection);
                    command.Parameters.AddWithValue("@CI", ChooseContact.ToString());

                    connection.Open();
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM `MessageBase` WHERE `ChatID` = @CI";
                    command.ExecuteNonQuery();
                }
            }
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
    public class Commands()
    {
        private DataBase db = new DataBase();
        public async Task LoadMessages(ListBox MessagesField, TextBox messageField, Button sendButton)
        {
            using DataTable MessageList = new DataTable();
            string CurrentDate = null;

            using (var connection = db.GetNewConnection())
            {
                using (var command = new MySqlCommand("SELECT * FROM `MessageBase` WHERE `ChatID` = @CI ORDER BY Date ASC, Time ASC", connection))
                {
                    command.Parameters.Add("@CI", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact;
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        connection.Open();
                        adapter.Fill(MessageList);
                    }
                }
            }

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

            await hubConnection.InvokeAsync("SendMessage", NeuronMain.ChooseContact.ToString(), message);
            using (var connection = db.GetNewConnection()) 
            {
                connection.Open();

                using (var command = new MySqlCommand(
                    "INSERT INTO `MessageBase` ( `ChatID`,`Sender`, `Message`, `Time`, `Date`) VALUES (@CI ,@S, @M, @T, @D )",
                    connection))
                {
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
            }
        }
        public void LoadContacts(NeuronMain neuronMain, ListBox chatListBox)
        {
            using DataTable ContactsList = new DataTable();
            using (var connection = db.GetNewConnection())
            {
                using (var command = new MySqlCommand("SELECT * FROM `contactbase` WHERE `Member` = @Username", connection))
                {
                    command.Parameters.Add("@Username", MySqlDbType.VarChar).Value = MainWindow.Login;
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        connection.Open();
                        adapter.Fill(ContactsList);
                    }
                }
            }
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
}