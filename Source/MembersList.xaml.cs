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
            if (AdminCheck() == 0)
            {
                MessageBox.Show("Недостаточно прав", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                AddMember addMember = new AddMember();
                addMember.Show();
            }
        }
        private void MakeAdmin(object sender, RoutedEventArgs e)
        {
            if (AdminCheck() == 0)
            {
                MessageBox.Show("Недостаточно прав", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {

            }
        }
        private void DeleteMember(object sender, RoutedEventArgs e)
        {
            if (AdminCheck() == 0)
            {
                MessageBox.Show("Недостаточно прав", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {

            }
        }
        private int AdminCheck()
        {
            using (var connection = db.GetNewConnection())
            {
                using (var command = new MySqlCommand("SELECT `IsAdmin` FROM `contactbase` WHERE `ChatID` = @CI AND `Member` = @ME", connection))
                {
                    command.Parameters.Add("@CI", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact.ToString();
                    command.Parameters.Add("@ME", MySqlDbType.VarChar).Value = MainWindow.Login;
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        using (DataTable table = new DataTable())
                        {
                            connection.Open();
                            adapter.Fill(table);
                            if (Convert.ToInt16(table.Rows[0][0]) == 0)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1;
                            }
                        }
                    }
                }
            }
        }
        private void LoadContacts()
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
