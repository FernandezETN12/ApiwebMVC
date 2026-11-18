namespace ApiwebMVC.Services;

/// <summary>Decide quién es administrador a partir de la lista "Admin:Emails" de la configuración.</summary>
public interface IAdminService
{
    bool EsAdmin(string? email);
}

public sealed class AdminService : IAdminService
{
    private readonly HashSet<string> _emails;

    public AdminService(IConfiguration configuration)
    {
        var emails = configuration.GetSection("Admin:Emails").Get<string[]>() ?? [];
        _emails = emails
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Select(email => email.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public bool EsAdmin(string? email) =>
        !string.IsNullOrWhiteSpace(email) && _emails.Contains(email.Trim());
}
