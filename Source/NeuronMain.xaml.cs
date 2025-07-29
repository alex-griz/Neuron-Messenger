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
            Commands.LoadContacts(this);
            ChooseContact = null;
            while (true)
            {
                if (ChooseContact != null)
                {
                    Commands.UpdateMessages();
                }
            }
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
        public static void LoadContacts(NeuronMain neuronMain)
        {
            string[] Contacts = new string[128];
            ListBox ChatBox = new ListBox();
            DataBase db = new DataBase();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `ContactsBase` WHERE `Owner` = @Username",
            db.getConnection());

            command.Parameters.Add("@Username", MySqlDbType.VarChar).Value = MainWindow.Login;
            adapter.SelectCommand = command;
            adapter.Fill(dataTable);

            for (int i = 0; i < Contacts.Length; i++)
            {
                try
                {
                    Contacts[i] = dataTable.Rows[2][i].ToString();
                    Button button = new Button() { Content = Contacts[i] };

                    ChatBox.Items.Add(button);
                    neuronMain.ChooseContact = dataTable.Rows[1][i].ToString();
                    //отображение контакта в списке
                }
                catch
                {
                    break;
                }
            }
        }
    }
}