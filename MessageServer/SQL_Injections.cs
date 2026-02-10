using MySql.Data.MySqlClient;
using System.Data;
namespace NeuronServer
{
    public class SQL_Injections
    {
        private static DataBase db = new DataBase();
        public static bool AdminCheck(int ChatId, string username)
        {
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand("SELECT `Role` FROM `ContactBase` WHERE `ChatID` = @CI AND `Member` = @ME", connection);
            using var adapter = new MySqlDataAdapter(command);
            using var result = new DataTable();

            command.Parameters.AddWithValue("@CI" , ChatId);
            command.Parameters.AddWithValue("@ME", username);

            connection.OpenAsync();
            adapter.Fill(result);

            if (result.Rows[0][0].ToString() == "1")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static int AddMember(int ChatId, string username, string target_member)
        {
            if(AdminCheck(ChatId, username))
            {
                try
                {
                    using var connection = db.GetNewConnection();
                    using var command = new MySqlCommand("INSERT INTO `contactbase` (`ChatID`, `Member`, `Role`) VALUES (@CI , @ME, @R)"
                    , connection);
                    
                    command .Parameters.AddWithValue("@CI", ChatId);
                    command.Parameters.AddWithValue("@ME", target_member);
                    command.Parameters.AddWithValue("@R", 0);

                    connection.OpenAsync();
                    command.ExecuteNonQuery();

                    return 1;
                }
                catch
                {
                    return 2;
                }
            }
            else
            {
                return 0;
            }
        }
        public static int MakeAdmin(int ChatId, string username, string target_member)
        {
            if(AdminCheck(ChatId, username))
            {
                try
                {
                    
                }
                catch
                {
                    return 2;
                }
            }
            else
            {
                return 0;
            }
        }
        public static int DeleteMember(int ChatId, string username, string target_member)
        {
            if(AdminCheck(ChatId, username))
            {
                try
                {
                    
                }
                catch
                {
                    return 2;
                }
            }
            else
            {
                return 0;
            }
        }
        public static int DeleteChat(int ChatId, string username)
        {
            if(AdminCheck(ChatId, username))
            {
                try
                {
                    using var connection = db.GetNewConnection();
                    using var command = new MySqlCommand("DELETE FROM `ContactBase` WHERE `ChatID` = @CI", connection);
                    command.Parameters.AddWithValue("@CI", ChatId);

                    connection.Open();
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM `MessageBase` WHERE `ChatID` = @CI";
                    command.ExecuteNonQuery();

                    return 2;
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                try
                {
                    using var connection = db.GetNewConnection();
                    using var command = new MySqlCommand("DELETE FROM `ContactBase` WHERE `Member` = @ME AND `ChatID` = @CI", connection);
                    command.Parameters.AddWithValue("@ME", username);
                    command.Parameters.AddWithValue("@CI", ChatId);

                    connection.Open();
                    command.ExecuteNonQuery();

                    return 1;
                }
                catch
                {
                    return 0;
                }
            }
        }
    }
}