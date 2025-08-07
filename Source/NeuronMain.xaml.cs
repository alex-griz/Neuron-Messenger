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
        public string ChooseContact {  get; set; }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddContact window = new AddContact();
            window.Show();
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
        public static void SendMessage()
        {

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