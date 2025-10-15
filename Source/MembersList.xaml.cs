using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<CheckItem> users = new ObservableCollection<CheckItem>();
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
                var SelectedUsers = users.Where(u => u.IsSelected).Select(u => u.Name).ToArray();
                using (var connection = db.GetNewConnection())
                {
                    using (var command = new MySqlCommand("UPDATE `contactbase` SET `IsAdmin` = 1 WHERE `ChatID` = @CI AND `Member` = @ME", connection))
                    {
                        command.Parameters.AddWithValue("@CI", NeuronMain.ChooseContact.ToString());
                        command.Parameters.Add("@ME", MySqlDbType.VarChar);
                        connection.Open();
                        foreach(string username  in SelectedUsers)
                        {
                            command.Parameters["@ME"].Value = username; 
                            command.ExecuteNonQuery();
                        }
                    }
                    SelectedUsers = null;
                }
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
                var SelectedUsers = users.Where(u => u.IsSelected).Select(u => u.Name).ToArray();
                using (var connection = db.GetNewConnection())
                {
                    using (var command = new MySqlCommand("DELETE FROM `contactbase` WHERE `ChatID` = @CI AND `Member` = @ME", connection))
                    {
                        command.Parameters.AddWithValue("@CI", NeuronMain.ChooseContact.ToString());
                        command.Parameters.Add("@ME", MySqlDbType.VarChar);
                        connection.Open();

                        foreach (string username in SelectedUsers)
                        {
                            command.Parameters["@ME"].Value = username;
                            command.ExecuteNonQuery();
                        }
                    }
                    SelectedUsers = null;
                }
            }
        }
        private void LoadContacts()
        {
            users.Clear();
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
            foreach(DataRow row in ContactsList.Rows)
            {
                users.Add(new CheckItem { Name = row[0].ToString(), IsSelected = false });
            }

            MembersListBox.ItemsSource = users;
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

    }
    public class CheckItem()
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
