using Microsoft.AspNetCore.SignalR;
using System;

namespace NeuronServer.Hubs;

public class ChatHub: Hub
{
    public override async Task OnConnectedAsync() //подключение клиента в хаб
    {
        await base.OnConnectedAsync();
        Console.WriteLine("User connected");
    }
    public override async Task OnDisconnectedAsync(Exception exception) //отключение клиента из хаба
    {
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine("User Disconnected");
    }
    public async Task JoinChat(string ChatId) //присоединение пользователя в группу, создание группы
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ChatId);
        Console.WriteLine("User joined the Chat");
    }
    public async Task LeaveChat(string ChatId) //удаление пользователя из группы
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ChatId);
        Console.WriteLine("User left the chat");
    }
    public async Task SendMessage(string ChatId, ChatMessage message) //отправка сообщения в выбранную группу
    {
        await Clients.Group(ChatId).SendAsync("GetMessage", message);
        Console.WriteLine("Message Sent to group");
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