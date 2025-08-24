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
            NeuronMain.ChooseContact = selectContactName;

            HeadNameLabel.Content = selectContactName;
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
        public static void LoadMessages()
        {

        }
        public static void UpdateMessages()
        {

        }
        public static void SendMessage(string MessageText)
        {
            DataBase db = new DataBase();
            MySqlCommand command = new MySqlCommand("INSERT INTO `MessageBase` (`Recipient`, `Sender`, `Time`, `Message`) VALUES (@R, @S, @T, @M)",
            db.getConnection());

            command.Parameters.Add("@R", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact;
            command.Parameters.Add("@S" , MySqlDbType.VarChar).Value = MainWindow.Login;
            command.Parameters.Add("@T", MySqlDbType.DateTime).Value = DateTime.Now;
            command.Parameters.Add("@M", MySqlDbType.Text).Value = MessageText;

            db.OpenConnection();
            command.ExecuteNonQuery();
            db.CloseConnection();
        }
        public static void LoadContacts(NeuronMain neuronMain, ListBox chatListBox)
        {
            string[] Contacts = new string[128];
            DataBase db = new DataBase();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            DataTable ContactsList = new DataTable();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `ContactBase` WHERE `Owner` = @Username",
            db.getConnection());

            command.Parameters.Add("@Username", MySqlDbType.VarChar).Value = MainWindow.Login;
            adapter.SelectCommand = command;
            adapter.Fill(ContactsList);

            for (int i = 0; i < Contacts.Length; i++)
            {
                try
                {
                    Contacts[i] = ContactsList.Rows[i][2].ToString();
                    chatListBox.Items.Add(Contacts[i]);
                }
                catch
                {
                    break;
                }
            }
        }
    }
}