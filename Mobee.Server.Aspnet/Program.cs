using Mobee.Server.Aspnet.Hubs;

namespace Mobee.Server.Aspnet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // This is important for SignalR!
                });
            });

            builder.Services.AddSingleton(typeof(UsersRepository));
            builder.Services.AddRazorPages();
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.MapFallbackToFile("/web/{*path}", "web/index.html");

            app.UseCors();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapHub<PlayersHub>("/playersHub");

            app.Run();
        }
    }
}