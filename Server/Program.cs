using Microsoft.AspNetCore.SignalR;

namespace NeuronServer
{
    public class Program
    {
        public static void Main()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddSignalR();
            
            var app = builder.Build();
            app.Run();
        }
    }
}
