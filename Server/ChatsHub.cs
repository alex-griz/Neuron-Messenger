using Microsoft.AspNetCore.SignalR;

namespace NeuronServer.Hubs;

public class ChatHub: Hub
{
    public override async Task OnConnectedAsync() //подключение клиента в хаб
    {
        await base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception exception) //отключение клиента из хаба
    {
        await base.OnDisconnectedAsync(exception);
    }
    public async Task JoinChat(string ChatId) //присоединение пользователя в группу, создание группы
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ChatId);
    }
    public async Task LeaveChat(string ChatId) //удаление пользователя из группы
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ChatId);
    }
    public async Task SendMessageToGroup(string ChatId, ChatMessage message) //отправка сообщения в выбранную группу
    {
        await Clients.Group(ChatId).SendAsync("GetMessage", message);
    }
    public async Task SendMessageToChat(string ChatId, ChatMessage message)
    {
        await Clients.Client(ChatId).SendAsync("GetMessage", message);
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