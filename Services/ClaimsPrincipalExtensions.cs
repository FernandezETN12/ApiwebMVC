using System.Security.Claims;

namespace ApiwebMVC.Services;

public static class ClaimsPrincipalExtensions
{
    public const string ClienteIdClaim = "ClienteId";

    public static int? GetClienteId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirst(ClienteIdClaim)?.Value, out var id) ? id : null;

    public static bool EsAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(Models.Roles.Admin);
}
