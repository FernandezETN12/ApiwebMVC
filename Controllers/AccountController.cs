using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ApiwebMVC.Models;
using ApiwebMVC.Services;

namespace ApiwebMVC.Controllers;

public sealed class AccountController : Controller
{
    public const string ExternalScheme = "External";

    private readonly IConfiguration _configuration;
    private readonly IClienteService _clienteService;
    private readonly IAdminService _adminService;
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        IConfiguration configuration,
        IClienteService clienteService,
        IAdminService adminService,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _clienteService = clienteService;
        _adminService = adminService;
        _environment = environment;
    }

    private bool GoogleConfigured =>
        !string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientId"]) &&
        !string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientSecret"]);

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["GoogleConfigured"] = GoogleConfigured;
        ViewData["DevLoginEnabled"] = _environment.IsDevelopment();
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChallengeGoogle(string? returnUrl = null)
    {
        if (!GoogleConfigured)
            return RedirectToAction(nameof(Login));

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback), "Account", new { returnUrl })
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
    {
        // Google deja su identidad en una cookie temporal ("External"); la leemos y la borramos.
        var googleResult = await HttpContext.AuthenticateAsync(ExternalScheme);
        await HttpContext.SignOutAsync(ExternalScheme);
        if (!googleResult.Succeeded || googleResult.Principal is null)
            return RedirectToAction(nameof(Login));

        var principal = googleResult.Principal;
        var name = principal.FindFirstValue(ClaimTypes.Name) ?? "Cliente";
        var email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var googleSubject = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? email;

        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction(nameof(Login));

        await SignInClienteAsync(googleSubject, name, email);
        return RedirectToLocal(returnUrl);
    }

    /// <summary>
    /// Login de prueba SOLO para desarrollo: permite entrar con cualquier email sin Google
    /// para probar los roles de cliente y admin. En producción devuelve 404.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DevLogin(string email, string? nombre, string? returnUrl = null)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        email = email?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            TempData["LoginError"] = "Ingresá un email válido.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var displayName = string.IsNullOrWhiteSpace(nombre) ? email.Split('@')[0] : nombre.Trim();
        await SignInClienteAsync($"dev:{email.ToLowerInvariant()}", displayName, email);
        return RedirectToLocal(returnUrl);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Perfil([FromServices] IPedidoService pedidoService)
    {
        var clienteId = User.GetClienteId();
        if (clienteId is null)
            return RedirectToAction(nameof(Login));

        var pedidos = await pedidoService.GetByClienteAsync(clienteId.Value);
        ViewBag.Nombre = User.Identity?.Name;
        ViewBag.Email = User.FindFirst(ClaimTypes.Email)?.Value;
        ViewBag.ClienteId = clienteId;
        ViewBag.Rol = User.EsAdmin() ? Roles.Admin : Roles.Cliente;
        return View(pedidos);
    }

    private async Task SignInClienteAsync(string subject, string name, string email)
    {
        var cliente = await _clienteService.CreateOrUpdateFromGoogleAsync(subject, name, email);
        var rol = _adminService.EsAdmin(email) ? Roles.Admin : Roles.Cliente;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, subject),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, rol),
            new(ClaimsPrincipalExtensions.ClienteIdClaim, cliente.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            });
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}
