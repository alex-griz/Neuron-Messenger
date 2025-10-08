using MySql.Data.MySqlClient;
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

namespace Neuron
{
    public partial class MembersList : Window
    {
        DataBase db = new DataBase();
        public MembersList()
        {
            InitializeComponent();
            LoadContacts();
        }
        private void AddMember(object sender, RoutedEventArgs e)
        {
            AddMember addMember = new AddMember();
            addMember.Show();
        }
        public void LoadContacts()
        {
            DataTable ContactsList = new DataTable();
            using (var connection = db.GetNewConnection())
            {
                using (var command = new MySqlCommand("SELECT `Member` FROM `contactbase` WHERE `ChatID` = @CI", connection))
                {
                    command.Parameters.Add("@CI", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact;
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        connection.Open();
                        adapter.Fill(ContactsList);
                    }
                }
            }
            for (int i = 0; i < ContactsList.Rows.Count; i++)
            {
                MembersListBox.Items.Add("@"+ContactsList.Rows[i][0].ToString());
            }
        }
    }
}
