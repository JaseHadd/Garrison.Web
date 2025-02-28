using System.Security.Claims;
using Garrison.Lib;
using Garrison.Lib.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;

        config.AddUserSecrets<Program>();

        ConfigureServices(builder.Services, config);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            // app.UseHsts();
        }

        // app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, ConfigurationManager config)
    {
        string connectionString = config.GetConnectionString("Default")!;

        services.Configure<RouteOptions>(o => {
            o.LowercaseUrls = true;
            o.LowercaseQueryStrings = true;
        });
        
        services.AddRazorPages(o =>
        {
            o.Conventions.AuthorizePage("/Profile/Index");
        });

        services.AddDbContext<GarrisonContext>(o => {
            o.UseMySQL(connectionString);
            o.LogTo(Console.WriteLine);
        });

        services.AddAuthorization();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o => {
                    o.LoginPath = "/auth/login";
                    o.LogoutPath = "/auth/logout";
                });
        services.AddAuthentication(o => o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme);
        services.AddTransient<IUserManager, UserManager>();
        services.AddTransient<PasswordHasher>();

    }
}

class DiscordSettings
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
}

public interface IUserManager
{
    public Task<bool> SignIn(HttpContext httpContext, string username, string password);
    public Task<bool> SignOut(HttpContext httpContext);
}

class UserManager(GarrisonContext dbContext, PasswordHasher hasher) : IUserManager
{
    private readonly GarrisonContext    _dbContext = dbContext;
    private readonly PasswordHasher     _passwordHasher = hasher;

    public async Task<bool> SignIn(HttpContext httpContext, string userName, string password)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserName == userName);

        if (user is not null && _passwordHasher.VerifyPassword(user, password))
        {
            await httpContext.SignInAsync(
                scheme: CookieAuthenticationDefaults.AuthenticationScheme,
                principal: new ClaimsPrincipal(
                    new ClaimsIdentity(GetUserClaims(user), "Basic")
                )
            );

            return true;
        }

        return false;
    }

    public async Task<bool> SignOut(HttpContext httpContext)
    {
        await httpContext.SignOutAsync();
        return true;
    }

    private IEnumerable<Claim> GetUserClaims(User user) => [
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        ..GetUserRoleClaims(user)
    ];

    private IEnumerable<Claim> GetUserRoleClaims(User user) => [];
}