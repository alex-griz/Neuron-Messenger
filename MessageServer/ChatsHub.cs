using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using MySql.Data.MySqlClient;

namespace NeuronServer.Hubs;

public class ChatHub: Hub
{
    public static ConcurrentDictionary<string , string> connections = new();
    DataBase db = new DataBase();
    public override async Task OnConnectedAsync() 
    {
        var username = Context.User?.FindFirst("username")?.Value;
        connections[Context.ConnectionId] = username!;
        
        await base.OnConnectedAsync();
        await Groups.AddToGroupAsync(Context.ConnectionId, username!);
        UserCache.OnlineStatus[username!] = true;

        Console.WriteLine("User connected");
    }
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = connections[Context.ConnectionId];

        await base.OnDisconnectedAsync(exception);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, username);
        UserCache.OnlineStatus[username] = false;

        Console.WriteLine("User Disconnected");
    }
    public async Task SendMessage(int ChatId, ChatMessage message)
    {
        foreach(string member in UserCache.ChatMembers[ChatId])
        {
            if (UserCache.OnlineStatus[member])
            {
                await Clients.Group(member).SendAsync("GetMessage", message);
            }
        }
        using var connection = db.GetNewConnection();
        using var command = new MySqlCommand("INSERT INTO `MessageBase` ( `ChatID`,`Sender`, `Message`, `Time`, `Date`, `Iv`) VALUES (@CI ,@S, @M, @T, @D, @Iv )", connection);

        command.Parameters.Add("@S", MySqlDbType.VarChar).Value = message.Sender;
        command.Parameters.Add("@T", MySqlDbType.VarChar).Value = message.Time;
        command.Parameters.Add("@M", MySqlDbType.LongBlob).Value = message.Message;
        command.Parameters.Add("@CI", MySqlDbType.Int32).Value = message.ChatID;
        command.Parameters.Add("@D", MySqlDbType.VarChar).Value = message.Date;
        command.Parameters.AddWithValue("@Iv", message.Iv);

        try
        {
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch
        {
            Console.WriteLine("Не удалось сохранить сообщение в БД");
        }
    }
}
public class ChatMessage
{
    public int ChatID {get; set;}
    public string Sender {get; set;}
    public byte[] Message {get; set;}
    public string Time {get; set;}
    public string Date {get; set;}
    public byte[] Iv { get; set; }
}