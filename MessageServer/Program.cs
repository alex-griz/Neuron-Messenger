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
            app.MapPost("/AddMember", (int ChatId, string username, string target_member, HttpContext context) => SQL_Injections.AddMember(ChatId, target_member, context)).RequireAuthorization();
            app.MapGet("/DeleteChat", (int ChatId, HttpContext context) => SQL_Injections.DeleteChat(ChatId, context)).RequireAuthorization();


            
            await UserCache.LoadUsersData();
            
            app.Run();
        }
    }
}