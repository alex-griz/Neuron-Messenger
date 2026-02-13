using Microsoft.AspNetCore.SignalR;
using NeuronServer.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
            builder.Services.AddAuthorization();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "NeuronAuthServer",
                    ValidateAudience = true,
                    ValidAudience = "NeuronClient",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("w904myu5*7my9xfgkh&^$*kas@#)_(gofHU&%oe")), 
                    ClockSkew = TimeSpan.Zero
                };
            });
            
            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRouting();
            app.UseCors("AllowAll");



            app.MapHub<ChatHub>("/chatHub");
            app.MapGet("/Login", (string username, string password) => SQL_Injections.Login(username, password));
            app.MapGet("/AddMember", (int ChatId, string username, string target_member) => SQL_Injections.AddMember(ChatId,username, target_member));
            app.MapGet("/DeleteChat", (int ChatId, string username) => SQL_Injections.DeleteChat(ChatId, username));


            
            await UserCache.LoadUsersData();
            
            app.Run();
        }
    }
}