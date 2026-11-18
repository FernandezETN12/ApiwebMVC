using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ApiwebMVC.Services;

namespace ApiwebMVC.Controllers;

public sealed class AccountController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IClienteService _clienteService;

    public AccountController(IConfiguration configuration, IClienteService clienteService)
    {
        _configuration = configuration;
        _clienteService = clienteService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["GoogleConfigured"] =
            !string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientId"]) &&
            !string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientSecret"]);
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChallengeGoogle(string? returnUrl = null)
    {
        var clientId = _configuration["Authentication:Google:ClientId"];
        var clientSecret = _configuration["Authentication:Google:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
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
        var googleResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!googleResult.Succeeded || googleResult.Principal is null)
            return RedirectToAction(nameof(Login));

        var principal = googleResult.Principal;
        var name = principal.FindFirstValue(ClaimTypes.Name) ?? "Cliente";
        var email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var googleSubject = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? email;

        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction(nameof(Login));

        var cliente = await _clienteService.CreateOrUpdateFromGoogleAsync(googleSubject, name, email);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, googleSubject),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email),
            new("ClienteId", cliente.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var localPrincipal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            localPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            });

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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Perfil([FromServices] IPedidoService pedidoService)
    {
        var clienteIdClaim = User.FindFirst("ClienteId")?.Value;
        if (!int.TryParse(clienteIdClaim, out var clienteId))
            return RedirectToAction(nameof(Login));

        var pedidos = await pedidoService.GetByClienteAsync(clienteId);
        ViewBag.Nombre = User.Identity?.Name;
        ViewBag.Email = User.FindFirst(ClaimTypes.Email)?.Value;
        ViewBag.ClienteId = clienteId;
        return View(pedidos);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}
