using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace NeuronServer
{
    public static class SQL_Injections
    {
        private static DataBase db = new DataBase();
        private static SymmetricSecurityKey token_key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("w904myu5*7my9xfgkh&^$*kas@#)_(gofHU&%oe"));
        private static readonly string token_issuer = "NeuronAuthServer";
        private static readonly string token_audience = "NeuronClient";
        private static SigningCredentials token_sign = new SigningCredentials(token_key, SecurityAlgorithms.HmacSha256);



        public static  LoginResult Login(string username, string password)
        {
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand("SELECT `Username` FROM `authbase` WHERE `Username` = @login AND `Password` = @password", connection);
            using var adapter = new MySqlDataAdapter(command);
            using var result = new DataTable();
            command.Parameters.AddWithValue("@login", username);
            command.Parameters.AddWithValue("@password", password);
            try
            {
                connection.Open();
                adapter.Fill(result);
                if (result.Rows.Count > 0)
                {
                    var user_claims = new[]
                    {
                        new Claim("username", username)
                    };
                    var token = new JwtSecurityToken(
                        issuer: token_issuer,
                        audience: token_audience,
                        claims: user_claims,
                        expires: DateTime.UtcNow.AddHours(1),
                        signingCredentials: token_sign
                    );
                    return new LoginResult
                    {
                        status = 1,
                        Jwt_token = new JwtSecurityTokenHandler().WriteToken(token) 
                    };
                }
                else
                {
                    return new LoginResult{
                        status =2,
                        Jwt_token = null
                    };
                }
            }
            catch
            {
                
                return new LoginResult{
                        status =0,
                        Jwt_token = null
                    };
            }


        }
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
        public static int AddMember(int ChatId, string target_member, HttpContext context)
        {
            string username = context.User.FindFirst("username")?.Value;
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

                    connection.Open();
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
        /*public static int MakeAdmin(int ChatId, string username, string target_member)
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
        }*/
        public static int DeleteChat(int ChatId, HttpContext context)
        {
            string username = context.User.FindFirst("username")?.Value;
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
public class LoginResult
{
    public int status;
    public string Jwt_token;
}