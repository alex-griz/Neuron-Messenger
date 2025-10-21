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
            NameBox.Text = NeuronMain.ChooseChatName;
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
                }
            }
        }
        private void LoadContacts()
        {
            users.Clear();
            using DataTable ContactsList = new DataTable();
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
            ContactButton selected = NeuronMain.clickedButton.DataContext as ContactButton;
            return selected.IsAdmin;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string NewChatName = NameBox.Text;
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand("UPDATE `ContactBase` SET `ChatName` = @CN WHERE `ChatID` = @CI", connection);
  
            command.Parameters.AddWithValue("@CN", NewChatName);
            command.Parameters.AddWithValue("@CI", NeuronMain.ChooseContact.ToString());
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Не удалось переименовать чат", "Ошибка отправки", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    public class CheckItem()
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
