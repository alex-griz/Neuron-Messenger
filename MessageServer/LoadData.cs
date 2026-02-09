using System.Collections.Concurrent;
using MySql.Data.MySqlClient;

namespace NeuronServer;

public static class UserCache
{
    public static ConcurrentDictionary<int, HashSet<string>> ChatMembers = new();
    public static ConcurrentDictionary<string, bool> OnlineStatus = new();
    public static async Task LoadUsersData()
    {
        string injectionString = "SELECT `ChatID`, `Member` FROM `ContactBase`";
        DataBase db = new DataBase();
        using var connection = db.GetNewConnection();

        await connection.OpenAsync();
        using var command = new MySqlCommand(injectionString, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            int ChatID = reader.GetInt32(0);
            string Member = reader.GetString(1);

            if (!ChatMembers.ContainsKey(ChatID))
            {
                ChatMembers[ChatID] = new HashSet<string>();
            }
            ChatMembers[ChatID].Add(Member);

            if (!OnlineStatus.ContainsKey(Member))
            {
                OnlineStatus[Member] = false;
            }
        } 
        Console.WriteLine("Users data loaded");
    }
}