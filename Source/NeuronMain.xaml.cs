using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Confluent.Kafka;
using System.IO;

namespace Neuron
{
    public partial class NeuronMain : Window
    {
        public static int ChooseContact;
        public static string ChooseChatName;
        public static ContactButton clicked = null;
        DataBase db = new DataBase();

        Commands commands = new Commands();

        private IProducer<string,string> producer;
        private IConsumer<string,string> consumer;

        public NeuronMain()
        {
            InitializeComponent();
            KafkaSet();
            commands.LoadContacts(this, ChatList);
        }
        private void KafkaSet()
        {
            var producerConfig = new ProducerConfig { BootstrapServers = "localhost:9092"};
            producer = new ProducerBuilder<string,string>(producerConfig).Build();

            var consumerConfig = new ConsumerConfig { 
                BootstrapServers = "localhost:9092" , 
                GroupId = "client" + MainWindow.Login,
                AutoOffsetReset = AutoOffsetReset.Latest
            };
            consumer = new ConsumerBuilder<string,string>(consumerConfig).Build();
        }
        private void LogOut(object sender ,RoutedEventArgs e)
        {
            MainWindow.Login = null;
            string json = null;
            File.WriteAllText("SavedLoginData.json", json);

            this.Close();
            MainWindow window = new MainWindow();
            window.Show();
        }
        private void SelectContact(object sender, RoutedEventArgs e)
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
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            commands.SendMessage(MessageField.Text);
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
        public string ButtonName {  get; set; }
        public int ChatID { get; set; }
        public int Type { get; set; }
        public int IsAdmin{get; set; }
    }
    public class Commands()
    {
        private DataBase db = new DataBase();
        public void LoadMessages(ListBox MessagesField, TextBox messageField, Button sendButton)
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
            if (MessageList.Rows.Count !=0)
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
        public void UpdateMessages()
        {
            
        }
        public void SendMessage(string MessageText)
        {
            using (var connection = db.GetNewConnection())  //отправка в Kafka сервис
            {
                connection.Open();

                using (var command = new MySqlCommand(
                    "INSERT INTO `MessageBase` ( `ChatID`,`Sender`, `Message`, `Time`, `Date`) VALUES (@CI ,@S, @M, @T, @D )",
                    connection))
                {
                    command.Parameters.Add("@S", MySqlDbType.VarChar).Value = MainWindow.Name;
                    command.Parameters.Add("@T", MySqlDbType.VarChar).Value = DateTime.Now.ToString().Substring(10,10);
                    command.Parameters.Add("@M", MySqlDbType.Text).Value = MessageText;
                    command.Parameters.Add("@CI", MySqlDbType.Int32).Value = NeuronMain.ChooseContact;
                    command.Parameters.Add("@D", MySqlDbType.VarChar).Value = DateTime.Now.ToString().Substring(0, 10);

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch
                    {
                        MessageBox.Show("Не удалось отправить сообщение","Ошибка отправки", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        public void LoadContacts(NeuronMain neuronMain, ListBox chatListBox)
        {
            using DataTable ContactsList = new DataTable();
            using (var connection = db.GetNewConnection())
            {
                using(var command = new MySqlCommand("SELECT * FROM `contactbase` WHERE `Member` = @Username", connection))
                {
                    command.Parameters.Add("@Username", MySqlDbType.VarChar).Value = MainWindow.Login;
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        connection.Open();
                        adapter.Fill(ContactsList);
                    }
                }
            }
            for(int i = 0; i < ContactsList.Rows.Count; i++)
            {
                var contact = new ContactButton
                {
                    ButtonName = ContactsList.Rows[i][2].ToString(),
                    ChatID = Convert.ToInt32(ContactsList.Rows[i][0]),
                    Type =Convert.ToInt16(ContactsList.Rows[i][3]),
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