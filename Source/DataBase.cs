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
        private string _connectionString;

        public DataBase()
        {
            _connectionString = "server=localhost; port=3306; username=root; password=root; database=NeuronDB";
        }

        public MySqlConnection GetNewConnection() 
        { 
            return new MySqlConnection(_connectionString); 
        }
    }
}
