using Microsoft.AspNetCore.SignalR;
using NeuronServer.Hubs;

namespace NeuronServer
{
    public class Program
    {
        public static void Main()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddSignalR();
            
            var app = builder.Build();
            app.UseRouting();
            app.MapHub<ChatHub>("/chatHub");
            
            app.Run();
        }
    }
}
