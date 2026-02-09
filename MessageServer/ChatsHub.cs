using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;

namespace NeuronServer.Hubs;

public class ChatHub: Hub
{
    public static ConcurrentDictionary<string , string> connections = new();
    DataBase db = new DataBase();
    public override async Task OnConnectedAsync() 
    {
        var username = Context.GetHttpContext().Request.Query["username"];
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
    }
}
public class ChatMessage
{
    public int ChatID {get; set;}
    public string Sender {get; set;}
    public string Message {get; set;}
    public string Time {get; set;}
    public string Date {get; set;}
}