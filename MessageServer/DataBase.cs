using MySql.Data.MySqlClient;
namespace NeuronServer;
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