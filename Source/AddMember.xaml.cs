using System;
using System.Collections.Generic;
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
    public partial class AddMember : Window
    {
        public AddMember()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Username = MemberUsername.Text;
            DataBase db = new DataBase();

            using (var connection = db.GetNewConnection())
            {
                using (var command = new MySqlCommand("INSERT INTO `contactbase` (`ChatID`, `Member`, `ChatName`) VALUES" +
                    "(@CI , @ME, @CN)", connection))
                {
                    command.Parameters.Add("@CI", MySqlDbType.VarChar).Value = NeuronMain.ChooseContact.ToString();
                    command.Parameters.Add("@ME", MySqlDbType.VarChar).Value = Username;
                    command.Parameters.Add("@CN", MySqlDbType.VarChar).Value = NeuronMain.ChooseChatName;
                    command.Parameters.Add("@IG", MySqlDbType.Int16).Value = 1;
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        this.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка при добавлении участника!", "Добавить участника", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
