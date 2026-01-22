using System.Windows;
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
                using (var command = new MySqlCommand("INSERT INTO `contactbase` (`ChatID`, `Member`, `Role`) VALUES" +
                    "(@CI , @ME, @R)", connection))
                {
                    command.Parameters.Add("@CI", MySqlDbType.Int16).Value = NeuronMain.ChooseContact;
                    command.Parameters.Add("@ME", MySqlDbType.VarChar).Value = Username;
                    command.Parameters.Add("@R", MySqlDbType.Int16).Value =0;
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
