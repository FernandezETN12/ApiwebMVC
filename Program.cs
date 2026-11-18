using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using ApiwebMVC.Data;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure ConnectionStrings:DefaultConnection with User Secrets or the " +
        "ConnectionStrings__DefaultConnection environment variable.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    // La API responde 401/403 en JSON en vez de redirigir a la página de login.
    options.Events.OnRedirectToLogin = context => RespondApiOrRedirect(context, StatusCodes.Status401Unauthorized);
    options.Events.OnRedirectToAccessDenied = context => RespondApiOrRedirect(context, StatusCodes.Status403Forbidden);
})
// Cookie temporal donde Google deja la identidad antes de que creemos la sesión propia.
.AddCookie(ApiwebMVC.Controllers.AccountController.ExternalScheme, options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
});

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            options.SignInScheme = ApiwebMVC.Controllers.AccountController.ExternalScheme;
            options.SaveTokens = true;
            options.CallbackPath = "/signin-google";
        });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(ApiwebMVC.Models.Roles.Admin, policy => policy.RequireRole(ApiwebMVC.Models.Roles.Admin));
});

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(4);
});
// Register application services
builder.Services.AddScoped<ApiwebMVC.Services.IPizzaService, ApiwebMVC.Services.PizzaService>();
builder.Services.AddScoped<ApiwebMVC.Services.IPedidoService, ApiwebMVC.Services.PedidoService>();
builder.Services.AddScoped<ApiwebMVC.Services.IClienteService, ApiwebMVC.Services.ClienteService>();
builder.Services.AddSingleton<ApiwebMVC.Services.IAdminService, ApiwebMVC.Services.AdminService>();
builder.Services.AddExceptionHandler<ApiwebMVC.Middleware.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Se encontraron uno o más errores de validación.",
            Instance = context.HttpContext.Request.Path
        });
    };
});

var app = builder.Build();

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await PizzaSeeder.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static Task RespondApiOrRedirect(Microsoft.AspNetCore.Authentication.RedirectContext<CookieAuthenticationOptions> context, int statusCode)
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new
        {
            status = statusCode,
            title = statusCode == StatusCodes.Status401Unauthorized
                ? "Tenés que iniciar sesión."
                : "No tenés permisos para esta acción."
        });
    }

    context.Response.Redirect(context.RedirectUri);
    return Task.CompletedTask;
}
