using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
namespace NeuronServer
{
    public static class SQL_Injections
    {
        private static DataBase db = new DataBase();
        private static SymmetricSecurityKey token_key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("w904myu5*7my9xfgkh&^$*kas@#)_(gofHU&%oe"));
        private static readonly string token_issuer = "NeuronAuthServer";
        private static readonly string token_audience = "NeuronClient";
        private static SigningCredentials token_sign = new SigningCredentials(token_key, SecurityAlgorithms.HmacSha256);



        public static int Registration(string username, string name, string password, string public_key, string private_key)
        {
            if (CheckUsername(username))
            {
                byte[] privateKeyBytes = Convert.FromBase64String(private_key);
                byte[] publicKeyBytes = Convert.FromBase64String(public_key);

                using var connection = db.GetNewConnection();
                using var command = new MySqlCommand("INSERT INTO `authbase` (`Username`, `Name`, `Password`, `Private_Key`) VALUES (@Username , @Name, @Password, @Private)", connection);

                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Password", password);
                command.Parameters.AddWithValue("@Private", privateKeyBytes);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO `ProfileBase` (`Username`, `Name`, `Public_Key`) VALUES (@Username , @Name, @Public)";
                    command.Parameters.AddWithValue("@Public", publicKeyBytes);
                    command.ExecuteNonQuery();
                    return 1;
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return 2;
            }
        } 
        private static bool CheckUsername(string username)
        {
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand("SELECT `Name` FROM `authbase` WHERE `Username` = @Username", connection);
            using var adapter = new MySqlDataAdapter(command);
            using var table = new DataTable();
            command.Parameters.AddWithValue("@Username", username);

            connection.Open();
            adapter.Fill(table);
            if (table.Rows.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static string Login(string username, string password)
        {
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand("SELECT `Username`, `Private_Key` FROM `authbase` WHERE `Username` = @login AND `Password` = @password", connection);
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
                    return JsonSerializer.Serialize(new {status = 1, token = new JwtSecurityTokenHandler().WriteToken(token) , private_encrypt_key = Convert.ToBase64String((byte[])result.Rows[0][1])});
                }
                else
                {
                    return JsonSerializer.Serialize(new {status = 2, token = "" , private_encrypt_key = ""});
                }
            }
            catch
            {
                 return JsonSerializer.Serialize(new {status = 0, token = "", private_encrypt_key = ""});
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

            connection.Open();
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
                    return 0;
                }
            }
            else
            {
                return 2;
            }
        }
        public static int MakeAdmin(int ChatId, string[] target_members ,HttpContext context)
        {
            string username = context.User?.FindFirst("username")?.Value;
            if(AdminCheck(ChatId, username))
            {
                using var connection = db.GetNewConnection();
                using var command = new MySqlCommand("UPDATE `contactbase` SET `Role` = 1 WHERE `ChatID` = @CI AND `Member` = @ME", connection);
                command.Parameters.AddWithValue("@CI", ChatId);
                command.Parameters.Add("@ME", MySqlDbType.VarChar);
                try
                {
                    connection.Open();
                    foreach (string targetUsername in target_members)
                    {
                        command.Parameters["@ME"].Value = targetUsername;
                        command.ExecuteNonQuery();
                    }
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
        public static int DeleteMember(int ChatId, string[] target_members,  HttpContext context)
        {
            string username = context.User?.FindFirst("username")?.Value;
            if(AdminCheck(ChatId, username))
            {
                using var connection = db.GetNewConnection();
                using var command = new MySqlCommand("DELETE FROM `contactbase` WHERE `ChatID` = @CI AND `Member` = @ME", connection);
                command.Parameters.AddWithValue("@CI", ChatId);
                command.Parameters.Add("@ME", MySqlDbType.VarChar);
                try
                {
                    connection.Open();
                    foreach (string targetUsername in target_members)
                    {
                        command.Parameters["@ME"].Value = targetUsername;
                        command.ExecuteNonQuery();
                    }
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

                    command.CommandText = "DELETE FROM `ChatBase` WHERE `ChatID` = @CI";
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
        public static int AddContact(string target_username, HttpContext context)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            byte[] key = aes.Key;

            UserCache.last_chat_id +=1;
            string username = context.User.FindFirst("username")?.Value;
            byte[] encrypted_key = Encrypt_key(key,username);

            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand("INSERT INTO `contactbase` (`ChatID`, `Member`,`SecondMember`, `Role`, `Aes`) VALUES (@CI , @ME,@SM, @R, @A)", connection);
            command.Parameters.Add("@CI", MySqlDbType.Int16).Value = UserCache.last_chat_id;
            command.Parameters.Add("@ME", MySqlDbType.VarChar).Value = username;
            command.Parameters.Add("@SM", MySqlDbType.VarChar).Value = target_username;
            command.Parameters.Add("@R", MySqlDbType.Int16).Value = 1;
            command.Parameters.AddWithValue("@A", encrypted_key);
            

            try
            {
                connection.Open();
                command.ExecuteNonQuery();

                command.Parameters["@ME"].Value = target_username;
                command.Parameters["@SM"].Value = username;
                command.Parameters["@A"].Value = Encrypt_key(key,target_username);
                command.ExecuteNonQuery();

                command.CommandText = "INSERT INTO `ChatBase` (`ChatID`, `ChatName`, `Description`, `Photo` , `Type`) VALUES (@CI, NULL, NULL, NULL, 0)";
                command.ExecuteNonQuery();

               return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static int AddGroup(string name, HttpContext context)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            byte[] key = aes.Key;
            byte[] encrypted_key  = Encrypt_key(key, context.User.FindFirst("username")?.Value);

            UserCache.last_chat_id +=1;
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand("INSERT INTO `contactbase` (`ChatID`, `Member`, `Role`, `Aes`) VALUES (@CI , @ME, @R, @A)", connection);
            
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@CI", UserCache.last_chat_id);
            command.Parameters.AddWithValue("@ME", context.User.FindFirst("username")?.Value);
            command.Parameters.AddWithValue("@R", 1);
            command.Parameters.AddWithValue("@A", encrypted_key);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();

                command.CommandText = "INSERT INTO `ChatBase` (`ChatID`, `ChatName`, `Description`, `Photo`, `Type`) VALUES (@CI, @CN, NULL, NULL, 1)";
                command.Parameters.AddWithValue("@CN", name);
                command.ExecuteNonQuery();

                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static int ChangeProfileData(string new_username, string new_name, string new_bio, HttpContext context)
        {
            var username = context.User.FindFirst("username")?.Value;
            using var connection = db.GetNewConnection();
            using var command = new MySqlCommand("UPDATE `ProfileBase` SET `Username` = @UN ,`Name` = @U , `Description` = @D WHERE `Username`= @UI", connection);

            command.Parameters.AddWithValue("@UN", new_username);
            command.Parameters.AddWithValue("@U", new_name);
            command.Parameters.AddWithValue("@D", new_bio);
            command.Parameters.AddWithValue("@UI", username);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();

                command.CommandText = "UPDATE `AuthBase` SET `Username` = @UN, `Name`= @U WHERE `Username` = @UI";
                command.ExecuteNonQuery();

                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static int DeleteMessage(int ChatID, string MessageID, HttpContext context)
        {
            var username = context.User.FindFirst("username")?.Value;
            if (AdminCheck(ChatID, username))
            {
                using var connection = db.GetNewConnection();
                using var command = new MySqlCommand("DELETE FROM `MessageBase` WHERE `MessageID` = @MI AND `ChatID` = @CI", connection);
                command.Parameters.AddWithValue("@MI", MessageID);
                command.Parameters.AddWithValue("@CI", ChatID);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return 1;
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return 2;
            }
        }
        public static byte[] Encrypt_key(byte[] key,string username)
        {
            using var conn = db.GetNewConnection();
            using var cmd = new MySqlCommand("SELECT `Public_Key` FROM `profilebase` WHERE Username = @U", conn);
            cmd.Parameters.AddWithValue("@U", username);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            reader.Read();
            byte[] creator_key = (byte[])reader["Public_Key"];

            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(creator_key, out _);
            return rsa.Encrypt(key, RSAEncryptionPadding.OaepSHA256);
        }
    }
}