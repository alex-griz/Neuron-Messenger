using Microsoft.AspNetCore.SignalR;
using NeuronServer.Hubs;

namespace NeuronServer
{
    public class Program
    {
        public static async Task Main()
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

            await UserCache.LoadUsersData();
            
            app.Run();
        }
    }
}