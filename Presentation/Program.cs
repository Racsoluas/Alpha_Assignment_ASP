using Business.Hubs;
using Business.Services;
using Data.Contexts;
using Data.Enteties.Notifications;
using Data.Entities;
using Data.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => !context.Request.Cookies.ContainsKey("cookieConsent");
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddIdentity<UserEntity, IdentityRole>(x =>
{
    x.User.RequireUniqueEmail = true;
    x.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(x =>
{
    x.LoginPath = "/auth/signin";
    x.AccessDeniedPath = "/auth/denied";
    #region ForExternalAuthentication
    x.Cookie.SameSite = SameSiteMode.None;
    x.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    #endregion
    x.Cookie.HttpOnly = true;
    x.Cookie.IsEssential = true;
    x.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    x.SlidingExpiration = true;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    })
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
        options.Scope.Add("user:email");
        options.Scope.Add("read.user");

        options.Events.OnCreatingTicket = async context =>
        {
            await Task.Delay(0);

            if (context.User.TryGetProperty("name", out var name))
            {
                var fullName = name.GetString();
                if (!string.IsNullOrEmpty(fullName))
                {
                    var names = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (names.Length > 0)
                    {
                        context.Identity?.AddClaim(new Claim(ClaimTypes.GivenName, names[0]));
                    }

                    if (names.Length > 1)
                    {
                        context.Identity?.AddClaim(new Claim(ClaimTypes.Surname, names[1]));
                    }
                }
            }
        };
    });

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

app.UseHsts();
app.UseHttpsRedirection();
app.UseRouting();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    string[] roleNames = ["Administrator", "User"];

//    foreach (var roleName in roleNames)
//    {
//        var exists = await roleManager.RoleExistsAsync(roleName);
//        if (!exists)
//        {
//            await roleManager.CreateAsync(new IdentityRole(roleName));
//        }
//    }

//    var db = scope.ServiceProvider.GetRequiredService<DataContext>();

//    string[] statusesToSeed = ["Scheduled", "In Progress", "Completed"];

//    foreach (var statusName in statusesToSeed)
//    {
//        var exists = await db.Status.AnyAsync(s => s.StatusName == statusName);
//        if (!exists)
//        {
//            db.Status.Add(new StatusEntity { StatusName = statusName });
//        }
//    }

//    string[] notificationTypesToSeed = ["User", "Project"];

//    foreach (var notificationType in notificationTypesToSeed)
//    {
//        var exists = await db.NotificationTypes.AnyAsync(s => s.NotificationType == notificationType);
//        if (!exists)
//        {
//            db.NotificationTypes.Add(new NotificationTypeEntity { NotificationType = notificationType });
//        }
//    }

//    string[] notificationTargetGroupsToSeed = ["AllUsers", "Admins"];

//    foreach (var notificationTargetGroup in notificationTargetGroupsToSeed)
//    {
//        var exists = await db.NotificationTargetGroups.AnyAsync(s => s.TargetGroup == notificationTargetGroup);
//        if (!exists)
//        {
//            db.NotificationTargetGroups.Add(new NotificationTargetGroupEntity { TargetGroup = notificationTargetGroup });
//        }
//    }

//    await db.SaveChangesAsync();
//}

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=auth}/{action=SignIn}/{id?}")
    .WithStaticAssets();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();