using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace Neuron
{
    public partial class NeuronMain : Window
    {
        private DataBase db = new DataBase();
        public static string ChooseContact;
        public NeuronMain()
        {
            InitializeComponent();
            Commands.LoadContacts(this, ChatList);
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
            db.getConnection());

            command.Parameters.Add("@O", MySqlDbType.VarChar).Value = MainWindow.Login;
            command.Parameters.Add("@C", MySqlDbType.VarChar).Value = selectContactName;

            adapter.SelectCommand = command;
            adapter.Fill(dataTable);

            NeuronMain.ChooseContact = dataTable.Rows[0][1].ToString();
            HeadNameLabel.Content = selectContactName;
            Commands.LoadMessages(MessagesField);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddContact window = new AddContact();
            window.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Commands.SendMessage(MessageField.Text);
            MessageField.Clear();
        }
    }
    public class Commands()
    {
        private static DataBase db = new DataBase();
        private static MySqlDataAdapter adapter = new MySqlDataAdapter();
        private static MySqlCommand command = new MySqlCommand();
        public static void LoadMessages(ListBox MessagesField)
        {
            DataTable MessageList = new DataTable();
            command = new MySqlCommand("SELECT * FROM `messagebase` WHERE `Recipient` = @R AND `Sender` = @S",
            db.getConnection());

            command.Parameters.Add("@R", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact;
            command.Parameters.Add("@S", MySqlDbType.VarChar).Value = MainWindow.Login;

            adapter.SelectCommand = command;
            adapter.Fill(MessageList);

            command.Parameters.Clear();
            command.Parameters.Add("@R", MySqlDbType.VarChar).Value = MainWindow.Login;
            command.Parameters.Add("@S", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact;

            adapter.Fill(MessageList);
            command.Parameters.Clear();

            MessageList.DefaultView.Sort = "Time ASC";
            DataView dataView = MessageList.DefaultView;
            DataTable SortedMessages = dataView.ToTable();

            for (int i=0; i<SortedMessages.Rows.Count; i++)
            {
                MessagesField.Items.Add(SortedMessages.Rows[i][1] + ":   " + SortedMessages.Rows[i][3]+ "   "+ SortedMessages.Rows[i][2]);
            }
        }
        public static void UpdateMessages()
        {

        }
        public static void SendMessage(string MessageText)
        {
            command = new MySqlCommand("INSERT INTO `MessageBase` (`Recipient`, `Sender`, `Time`, `Message`) VALUES (@R, @S, @T, @M)",
            db.getConnection());

            command.Parameters.Add("@R", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact;
            command.Parameters.Add("@S" , MySqlDbType.VarChar).Value = MainWindow.Login;
            command.Parameters.Add("@T", MySqlDbType.DateTime).Value = DateTime.Now;
            command.Parameters.Add("@M", MySqlDbType.Text).Value = MessageText;

            db.OpenConnection();
            command.ExecuteNonQuery();
            db.CloseConnection();

            command.Parameters.Clear();
        }
        public static void LoadContacts(NeuronMain neuronMain, ListBox chatListBox)
        {
            DataTable ContactsList = new DataTable();
            command = new MySqlCommand("SELECT * FROM `ContactBase` WHERE `Owner` = @Username",
            db.getConnection());

            command.Parameters.Add("@Username", MySqlDbType.VarChar).Value = MainWindow.Login;
            adapter.SelectCommand = command;
            adapter.Fill(ContactsList);

            for (int i = 0; i < ContactsList.Rows.Count; i++)
            {
                chatListBox.Items.Add(ContactsList.Rows[i][2].ToString());
            }
            command.Parameters.Clear();
        }
    }
}