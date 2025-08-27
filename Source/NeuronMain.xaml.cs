using MySql.Data.MySqlClient;
using Mysqlx.Crud;
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

namespace Neuron
{
    public partial class NeuronMain : Window
    {
        private DataBase db = new DataBase();
        public static string ChooseContact;
        Commands commands = new Commands();
        public NeuronMain()
        {
            InitializeComponent();
            commands.LoadContacts(this, ChatList);
            ChooseContact = null;
            /*while (true)
            {
                if (ChooseContact != null)
                {
                    Commands.UpdateMessages();
                }
            }*/
        }
        private void SelectContact(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            string selectContactName = clickedButton.Content.ToString();
            DataTable dataTable = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `contactbase` WHERE `Owner` = @O AND `ContactName` = @C",
            db.GetNewConnection());

            command.Parameters.Add("@O", MySqlDbType.VarChar).Value = MainWindow.Login;
            command.Parameters.Add("@C", MySqlDbType.VarChar).Value = selectContactName;

            adapter.SelectCommand = command;
            adapter.Fill(dataTable);

            ChooseContact = dataTable.Rows[0][1].ToString();
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
    }
    public class Commands()
    {
        private DataBase db = new DataBase();
        public void LoadMessages(ListBox MessagesField)
        {
            DataTable MessageList = new DataTable();
            using (var connection = db.GetNewConnection())
            {
                using (var command = new MySqlCommand("SELECT * FROM `MessageBase WHERE (`Recipient = @R AND `Sender` = @S) OR " +
                    "(`Recipient = @S AND `Sender` = @R)", connection))
                {
                    command.Parameters.Add("@R", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact;
                    command.Parameters.Add("@S", MySqlDbType.VarChar).Value = MainWindow.Login;
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
                MessagesField.Items.Add(SortedMessages.Rows[i][1] + "\n \n" + SortedMessages.Rows[i][3]+ "\n \n"+ SortedMessages.Rows[i][2]);
            }
        }
        public void UpdateMessages()
        {

        }
        public void SendMessage(string MessageText)
        {
            using (var connection = db.GetNewConnection())
            {
                connection.Open();

                using (var command = new MySqlCommand(
                    "INSERT INTO `MessageBase` (`Recipient`, `Sender`, `Time`, `Message`) VALUES (@R, @S, @T, @M)",
                    connection))
                {
                    command.Parameters.Add("@R", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact;
                    command.Parameters.Add("@S", MySqlDbType.VarChar).Value = MainWindow.Login;
                    command.Parameters.Add("@T", MySqlDbType.DateTime).Value = DateTime.Now;
                    command.Parameters.Add("@M", MySqlDbType.Text).Value = MessageText;

                    command.ExecuteNonQuery();
                }
            }
        }
        public void LoadContacts(NeuronMain neuronMain, ListBox chatListBox)
        {
            DataTable ContactsList = new DataTable();
            using (var connection = db.GetNewConnection())
            {
                using(var command = new MySqlCommand("SELECT * FROM `ContactBase` WHERE `Owner` = @Username", connection))
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
                chatListBox.Items.Add(ContactsList.Rows[i][2].ToString());
            }
        }
    }
}