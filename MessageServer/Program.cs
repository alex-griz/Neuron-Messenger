using NeuronServer.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MySqlX.XDevAPI.Common;

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
            app.MapPost("/MakeAdmin", (int ChatId, string[] target_members, HttpContext context) => SQL_Injections.MakeAdmin(ChatId, target_members, context)).RequireAuthorization();
            app.MapDelete("/DeleteMember", (int ChatId, string[] target_members, HttpContext context) => SQL_Injections.DeleteMember(ChatId, target_members, context)).RequireAuthorization();
            app.MapPost("/AddContact",(string target_username, HttpContext context) => SQL_Injections.AddContact(target_username, context)).RequireAuthorization();
            app.MapPost("/AddGroup", (string name, HttpContext context) => SQL_Injections.AddGroup(name,context)).RequireAuthorization();
            app.MapPost("/ChangeProfileData", (string username, string name, string bio, HttpContext context) => SQL_Injections.ChangeProfileData(username, name, bio, context)).RequireAuthorization();
            app.MapPost("/Reg", async (HttpContext context) =>
            {
                var form = await context.Request.ReadFormAsync();
    
                string username = form["username"];
                string name = form["name"];
                string password = form["password"];
                string public_key = form["public_key"];
                string private_key = form["private_key"];
                
                var result = SQL_Injections.Registration(username, name, password, public_key, private_key);
                return result.ToString();
            });
            app.MapPost("/Upload", async (string type, HttpContext context) =>
            {
                try
                {
                    string unique_name = $"{Guid.NewGuid():N}.{type}";
                    long total_bytes = 0;
                    byte[] buffer = new byte[32768];
                    using var file_stream = new FileStream(Path.Combine("FileStorage", unique_name), FileMode.Create, FileAccess.Write, FileShare.None, 32768, true);
                    using var body =  context.Request.Body;
                    while (true)
                    {
                        int bytesRead = await body.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
                        total_bytes += bytesRead;
                        if (total_bytes > 1024 * 1024 * 100)
                        {
                            File.Delete(Path.Combine("FileStorage", unique_name));
                            return Results.Text("2");
                        }
                        await file_stream.WriteAsync(buffer, 0, bytesRead);
                    }
                    return Results.Text(unique_name);
                }
                catch
                {
                    return Results.Text("0");
                }
            }).RequireAuthorization();
            app.MapGet("/Download", async (string file_name, HttpContext context) =>
            {
                string path = Path.Combine("FileStorage", file_name);
                if (!File.Exists(path))
                {
                    context.Response.StatusCode = 404;
                    return;
                }
                using var file_stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
                context.Response.ContentType = "application/octet-stream";
                context.Response.Headers.ContentDisposition = $"attachment; filename=\"{file_name}\"";
                await file_stream.CopyToAsync(context.Response.Body);
                
            }).RequireAuthorization();
            app.MapDelete("/DeleteMessage", (int ChatId, string MessageId, HttpContext context) => SQL_Injections.DeleteMessage(ChatId, MessageId, context)).RequireAuthorization();

            await UserCache.LoadUsersData();
            
            app.Run();
        }
    }
}