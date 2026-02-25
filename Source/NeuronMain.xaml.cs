using Microsoft.AspNetCore.SignalR.Client;
using MySql.Data.MySqlClient;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
namespace Neuron
{
    public partial class NeuronMain : Window
    {
        public static int ChooseContact;
        public static string ChooseChatName;
        public static ContactButton clicked = null;
        private static HubConnection hubConnection;
        public ConcurrentDictionary<int, ChatData> chatCache= new();
        public ConcurrentDictionary<string, UserData> userCache = new();

        DataBase db = new DataBase();

        Commands commands = new Commands();

        public NeuronMain()
        {
            InitializeComponent();

            hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5156/chatHub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(MainWindow.Jwt_Security_Token);
                })
                .WithAutomaticReconnect()
                .Build();

            hubConnection.On<ChatMessage>("GetMessage", (message) =>
            {
                if (message.ChatID == ChooseContact)
                {
                    commands.UpdateMessages(this, message);
                }
                else
                {
                    MessageBox.Show(message.Sender+message.Message, chatCache[message.ChatID].Name, MessageBoxButton.OK, MessageBoxImage.Information);
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

            commands.LoadMembers(this);
            await commands.LoadMessages(this);
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
        private async void DeleteChat(object sender, RoutedEventArgs e)
        {
            var answer = MessageBox.Show("Вы уверены что хотите безвозвратно удалить этот чат? Если вы не являетесь владельцем группы, вы просто покинете её", "Удаление чата", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(answer == MessageBoxResult.Yes)
            {
                var response = await MainWindow.client.DeleteAsync($"http://localhost:5156/DeleteChat?ChatId={ChooseContact}");
                var result = int.Parse(await response.Content.ReadAsStringAsync());
                switch (result)
                {
                    case 0:
                        MessageBox.Show("Ошибка на стороне сервера", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case 1:
                        MessageBox.Show("Вы успешно покинули группу", "Neuron", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case 2:
                        MessageBox.Show("Вы успешно удалили группу", "Neuron", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }
            }
        }
    }
    public class Commands()
    {
        private DataBase db = new DataBase();
        private string CurrentDate;
        public async Task LoadMessages(NeuronMain neuronMain)
        {
            using DataTable MessageList = new DataTable();

            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand(SQL_Injections.GetMessages, connection);
            using var adapter = new MySqlDataAdapter(command);

            command.Parameters.Add("@CI", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact;
            connection.Open();
            adapter.Fill(MessageList);

            neuronMain.MessagesField.Items.Clear();
            if (MessageList.Rows.Count != 0)
            {
                CurrentDate = MessageList.Rows[0][4].ToString();
                neuronMain.MessagesField.Items.Add(CurrentDate);
                for (int i = 0; i < MessageList.Rows.Count; i++)
                {
                    string sender = MessageList.Rows[i][1].ToString();
                    string message = MessageList.Rows[i][2].ToString();
                    string time = MessageList.Rows[i][3].ToString();
                    if (MessageList.Rows[i][4].ToString() == CurrentDate) 
                    {
                        neuronMain.MessagesField.Items.Add(neuronMain.userCache[sender].Name + "\n \n" + message + "\n \n" + time);
                    }
                    else
                    {
                        CurrentDate = MessageList.Rows[i][4].ToString();
                        neuronMain.MessagesField.Items.Add(CurrentDate);
                        neuronMain.MessagesField.Items.Add(neuronMain.userCache[sender].Name + "\n \n" + message + "\n \n" + time);
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
                    $"{neuronMain.userCache[chatMessage.Sender].Name}\n\n{chatMessage.Message}\n\n{chatMessage.Time}"
                );
            });
        }
        public async void SendMessage(string MessageText, HubConnection hubConnection)
        {
            ChatMessage message = new ChatMessage();
            message.ChatID = NeuronMain.ChooseContact;
            message.Sender = MainWindow.Login;
            message.Message = MessageText;
            message.Time = DateTime.Now.ToString("HH:mm");
            message.Date = DateTime.Now.ToString("dd.MM.yyyy");

            await hubConnection.InvokeAsync("SendMessage", NeuronMain.ChooseContact, message);
        }
        public async void LoadContacts(NeuronMain neuronMain, ListBox chatListBox)
        {
            using DataTable GroupContactsList = new DataTable();
            using DataTable UserContactsList = new DataTable();
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand(SQL_Injections.GetGroupContacts, connection);
            using var adapter = new MySqlDataAdapter(command);

            command.Parameters.Add("@Username", MySqlDbType.VarChar).Value = MainWindow.Login;

            connection.Open();
            adapter.Fill(GroupContactsList);

            command.CommandText = SQL_Injections.GetUserContacts;
            adapter.Fill(UserContactsList);

            var uniqueChats = new HashSet<int>();

            foreach (DataRow row in UserContactsList.Rows)
            {
                string name = row[1].ToString();
                int ChatID = Convert.ToInt32(row[0]);

                if (uniqueChats.Add(ChatID))
                {
                    neuronMain.chatCache[ChatID] = new ChatData
                    {
                        Name = name,
                        Type = 0,
                        ImagePath = ""
                    };
                    var contact = new ContactButton
                    {
                        ButtonName = name,
                        ChatID = ChatID,
                        Type = 0
                    };
                    chatListBox.Items.Add(contact);
                }
            }
            foreach (DataRow row in GroupContactsList.Rows)
            {
                string name = row[1].ToString();
                int ChatID = Convert.ToInt32(row[0]);
                int type = Convert.ToInt16(row[2]);

                if (uniqueChats.Add(ChatID))
                {
                    neuronMain.chatCache[ChatID] = new ChatData
                    {
                        Name = name,
                        Type = type,
                        ImagePath = ""
                    };
                    var contact = new ContactButton
                    {
                        ButtonName = name,
                        ChatID = ChatID,
                        Type = type
                    };
                    chatListBox.Items.Add(contact);
                }
            }
        }
        public async void LoadMembers(NeuronMain neuronMain)
        {
            MembersList.users.Clear();

            using DataTable table = new DataTable();
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand(SQL_Injections.LoadMembers, connection);
            using var adapter = new MySqlDataAdapter(command);

            command.Parameters.Add("@CI", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact;

            connection.Open();
            adapter.Fill(table);

            foreach (DataRow row in table.Rows)
            {
                string username = row[0].ToString();
                string name = row[1].ToString();
                try
                {
                    neuronMain.userCache[username].Name = name;
                }
                catch
                {
                    neuronMain.userCache[username] = new UserData{Name = name};
                }

                MembersList.users.Add(new CheckItem { Name = username, IsSelected = false });
            }
        }
    }
    public class ContactButton
    {
        public string ButtonName { get; set; }
        public int ChatID { get; set; }
        public int Type { get; set; }
    }
    public class ChatMessage
    {
        public int ChatID { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
    }
    public class ChatData()
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public string ImagePath { get; set; }
    }
    public class UserData()
    {
        public string Name { get; set; }
        //public string ImagePath { get; set; }
    }
}