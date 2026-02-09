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
            app.MapGet("/AddMember", (int ChatId, string username, string target_member) => SQL_Injections.AddMember(ChatId,username, target_member));
            app.MapGet("/DeleteChat", (int ChatId, string username) => SQL_Injections.DeleteChat(ChatId, username));
            
            await UserCache.LoadUsersData();
            
            app.Run();
        }
    }
}