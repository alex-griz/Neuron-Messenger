using Microsoft.AspNetCore.SignalR;
using NeuronServer.Hubs;

namespace NeuronServer
{
    public class Program
    {
        public static void Main()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:5156")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddSignalR();
            
            var app = builder.Build();
            app.UseRouting();
            app.UseCors("AllowAll");
            app.MapHub<ChatHub>("/chatHub");
            
            app.Run();
        }
    }
}
