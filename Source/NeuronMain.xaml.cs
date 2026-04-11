using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
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
                string message_text = Commands.DecryptMessage(message.Message, chatCache[message.ChatID].Aes_key, message.Iv);
                if (message.ChatID == ChooseContact)
                {
                    commands.UpdateMessages(this, message, message_text);
                }
                else
                {
                    MessageBox.Show(message.Sender+ message_text, chatCache[message.ChatID].Name, MessageBoxButton.OK, MessageBoxImage.Information);
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
            commands.SendMessage(this, MessageField.Text, hubConnection);
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
        private async void AttachFile(object sender, RoutedEventArgs e) 
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                string type = filePath.Split('.').Last();
                var content = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
                var response = await MainWindow.client.PostAsync($"http://localhost:5156/Upload?type={type}", content);
                var result = await response.Content.ReadAsStringAsync();

                switch (result)
                {
                    case "0":
                        MessageBox.Show("Ошибка во время загрузки файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "2":
                        MessageBox.Show("Размер файла превышает 100 Мб", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        break;
                }
            }
        }
        private void OpenProfileEditor(object sender, RoutedEventArgs e)
        {
            ProfileView.target_username = MainWindow.Login;
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
                ProfileView.target_username = members.Rows[0][0].ToString();
            }
            else
            {
                ProfileView.target_username = members.Rows[1][0].ToString();
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
        private async void DeleteMessage(object sender, RoutedEventArgs e)
        {
            var selected = MessagesField.SelectedItem as ListBoxItem;
            if (selected != null && selected.Tag != null)
            {
                var response = await MainWindow.client.DeleteAsync($"http://localhost:5156/DeleteMessage?ChatId={ChooseContact}&MessageId={selected.Tag.ToString()}");
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка: {response.StatusCode}\n{error}", "Ошибка", MessageBoxButton.OK);
                    return;
                }
                var result = int.Parse(await response.Content.ReadAsStringAsync());
                switch (result)
                {
                    case 0:
                        MessageBox.Show("Ошибка на стороне сервера", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case 1:
                        MessagesField.Items.Remove(MessagesField.SelectedItem);
                        break;
                    case 2:
                        MessageBox.Show("Недостаточно прав для удаления сообщений в этом чате", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                CurrentDate = MessageList.Rows[0][3].ToString();
                neuronMain.MessagesField.Items.Add(CurrentDate);
                for (int i = 0; i < MessageList.Rows.Count; i++)
                {
                    string sender = MessageList.Rows[i][1].ToString();
                    string message = DecryptMessage((byte[])MessageList.Rows[i][5], neuronMain.chatCache[NeuronMain.ChooseContact].Aes_key, (byte[])MessageList.Rows[i][4]);
                    string time = MessageList.Rows[i][2].ToString();
                    var item = new ListBoxItem();
                    item.Content = neuronMain.userCache[sender].Name + "\n \n" + message + "\n \n" + time;
                    if (MessageList.Rows[i][6].ToString() != null)
                    {
                        item.Tag = MessageList.Rows[i][6].ToString();
                    }
                    if (MessageList.Rows[i][3].ToString() == CurrentDate) 
                    {
                        neuronMain.MessagesField.Items.Add(item);
                    }
                    else
                    {
                        CurrentDate = MessageList.Rows[i][3].ToString();
                        neuronMain.MessagesField.Items.Add(CurrentDate);
                        neuronMain.MessagesField.Items.Add(item);
                    }
                }
            }
        }
        public void UpdateMessages(NeuronMain neuronMain, ChatMessage chatMessage, string decrypted_text)
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
                var item = new ListBoxItem();
                item.Content = $"{neuronMain.userCache[chatMessage.Sender].Name}\n\n{decrypted_text}\n\n{chatMessage.Time}";
                item.Tag = chatMessage.MessageID;
                neuronMain.MessagesField.Items.Add(item);
            });
        }
        public async void SendMessage(NeuronMain neuronMain, string MessageText, HubConnection hubConnection)
        {
            var encrypted_data = EncryptMessage(MessageText, neuronMain.chatCache[NeuronMain.ChooseContact].Aes_key);
            ChatMessage message = new ChatMessage();
            
            message.MessageID = Guid.NewGuid().ToString();
            message.ChatID = NeuronMain.ChooseContact;
            message.Sender = MainWindow.Login;
            message.Message = encrypted_data.Message;
            message.Time = DateTime.Now.ToString("HH:mm");
            message.Date = DateTime.Now.ToString("dd.MM.yyyy");
            message.Iv = encrypted_data.Iv;

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
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(MainWindow.private_key, out _);

            foreach (DataRow row in UserContactsList.Rows)
            {
                string name = row[1].ToString();
                int ChatID = Convert.ToInt32(row[0]);
                byte[] Aes = rsa.Decrypt((byte[])row[2], RSAEncryptionPadding.OaepSHA256);

                if (uniqueChats.Add(ChatID))
                {
                    neuronMain.chatCache[ChatID] = new ChatData
                    {
                        Name = name,
                        Type = 0,
                        ImagePath = "",
                        Aes_key = Aes
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
                byte[] Aes = rsa.Decrypt((byte[])row[3], RSAEncryptionPadding.OaepSHA256);

                if (uniqueChats.Add(ChatID))
                {
                    neuronMain.chatCache[ChatID] = new ChatData
                    {
                        Name = name,
                        Type = type,
                        ImagePath = "",
                        Aes_key = Aes
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
                string key = row[2].ToString();
                try
                {
                    neuronMain.userCache[username].Name = name;
                    neuronMain.userCache[username].public_key = key;
                }
                catch
                {
                    neuronMain.userCache[username] = new UserData{Name = name, public_key = key};
                }

                MembersList.users.Add(new CheckItem { Name = username, IsSelected = false });
            }
        }
        public static EncryptedMessage EncryptMessage(string message, byte[] chat_aes_key)
        {
            using var aes = Aes.Create();
            aes.Key = chat_aes_key;
            aes.GenerateIV();

            byte[] message_bytes = Encoding.UTF8.GetBytes(message);

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms , aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(message_bytes, 0, message_bytes.Length);
            cs.FlushFinalBlock();

            return new EncryptedMessage
            {
                Message = ms.ToArray(),
                Iv = aes.IV
            };
        }
        public static string DecryptMessage(byte[] encrypted_message, byte[] aes_key, byte[] Iv)
        {
            using var aes = Aes.Create();
            aes.Key = aes_key;
            aes.IV = Iv;

            using var ms = new MemoryStream(encrypted_message);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);

            return sr.ReadToEnd();
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
        public string MessageID { get; set;}
        public int ChatID { get; set; }
        public string Sender { get; set; }
        public byte[] Message { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
        public byte[] Iv { get; set; }
        //public string DataPath { get; set; }
    }
    public class ChatData()
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public string ImagePath { get; set; }
        public byte[] Aes_key { get; set; }
    }
    public class UserData()
    {
        public string Name { get; set; }
        public string public_key { get; set; }
        //public string ImagePath { get; set; }
    }
    public class EncryptedMessage
    {
        public byte[] Message { get; set; }
        public byte[] Iv { get; set; }
    }
}