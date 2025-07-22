using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Neuron
{
    class DataBase
    {
        MySqlConnection connection = new MySqlConnection("server=localhost; port= 3306; username=root; password=root; database = NeuronDB");

        public void OpenConnection()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }
        public MySqlConnection getConnection()
        {
            return connection;
        }
    }
}
