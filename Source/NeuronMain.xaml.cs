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

namespace Neuron
{
    public partial class NeuronMain : Window
    {
        public static int ChooseContact;
        public static string ChooseChatName;

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

        private void SelectContact(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            string selectContactName = clickedButton.Content.ToString();

            ChooseContact = Convert.ToInt32(clickedButton.Tag);
            ChooseChatName = selectContactName;
            HeadNameLabel.Content = selectContactName;
            commands.LoadMessages(MessagesField);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddContact window = new AddContact();
            window.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            commands.SendMessage(MessageField.Text);
            MessageField.Clear();
        }

        private void AddMemberButton_Click(object sender, RoutedEventArgs e)
        {
            AddMember addMember = new AddMember();
            addMember.Show();
        }
    }
    public class ContactButton()
    {
        public string ButtonName {  get; set; }
        public int ChatID { get; set; }
    }
    public class Commands()
    {
        private DataBase db = new DataBase();
        public void LoadMessages(ListBox MessagesField)
        {
            DataTable MessageList = new DataTable();
            using (var connection = db.GetNewConnection())
            {
                using (var command = new MySqlCommand("SELECT * FROM `MessageBase` WHERE `ChatID` = @CI", connection))
                {
                    command.Parameters.Add("@CI", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact;
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        connection.Open();
                        adapter.Fill(MessageList);
                    }
                }
            }

            MessageList.DefaultView.Sort = "Time ASC";
            DataView dataView = MessageList.DefaultView;
            DataTable SortedMessages = dataView.ToTable();

            for (int i=0; i<SortedMessages.Rows.Count; i++)
            {
                MessagesField.Items.Add(SortedMessages.Rows[i][1] + "\n \n" + SortedMessages.Rows[i][2]+ "\n \n"+ SortedMessages.Rows[i][3]);
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
                    "INSERT INTO `MessageBase` ( `ChatID`,`Sender`, `Message`, `Time`) VALUES (@CI ,@S, @M, @T )",
                    connection))
                {
                    command.Parameters.Add("@S", MySqlDbType.VarChar).Value = MainWindow.Name;
                    command.Parameters.Add("@T", MySqlDbType.DateTime).Value = DateTime.Now;
                    command.Parameters.Add("@M", MySqlDbType.Text).Value = MessageText;
                    command.Parameters.Add("@CI", MySqlDbType.Int32).Value = NeuronMain.ChooseContact;

                    command.ExecuteNonQuery();
                }
            }
        }
        public void LoadContacts(NeuronMain neuronMain, ListBox chatListBox)
        {
            DataTable ContactsList = new DataTable();
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
                    ChatID = Convert.ToInt32(ContactsList.Rows[i][0])
                };
                chatListBox.Items.Add(contact);
            }
        }
        public void UpdateContacts()
        {

        }
    }
}