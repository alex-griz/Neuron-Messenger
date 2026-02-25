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
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            
            var app = builder.Build();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowAll");



            app.MapHub<ChatHub>("/chatHub").RequireAuthorization();
            app.MapGet("/Login", (string username, string password) => SQL_Injections.Login(username, password));
            app.MapPost("/AddMember", (int ChatId, string target_member, HttpContext context) => SQL_Injections.AddMember(ChatId, target_member, context)).RequireAuthorization();
            app.MapDelete("/DeleteChat", (int ChatId, HttpContext context) =>  SQL_Injections.DeleteChat(ChatId, context)).RequireAuthorization();
            app.MapPost("/MakeAdmin", (int ChatId, string target_member, HttpContext context) => SQL_Injections.MakeAdmin(ChatId, target_member, context)).RequireAuthorization();
            app.MapPost("/AddContact",(string target_username, HttpContext context) => SQL_Injections.AddContact(target_username, context)).RequireAuthorization();
            app.MapPost("/AddGroup", (string name, HttpContext context) => SQL_Injections.AddGroup(name,context)).RequireAuthorization();
            

            
            await UserCache.LoadUsersData();
            
            app.Run();
        }
    }
}