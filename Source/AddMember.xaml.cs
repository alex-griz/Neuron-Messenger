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
        }
    }
}
