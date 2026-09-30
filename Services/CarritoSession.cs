using System.Text.Json;

namespace ApiwebMVC.Services;

public static class CarritoSession
{
    private const string SessionKey = "Carrito";

    public static Dictionary<int, int> Get(HttpContext context)
    {
        var json = context.Session.GetString(SessionKey);
        return string.IsNullOrWhiteSpace(json)
            ? new Dictionary<int, int>()
            : JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? new Dictionary<int, int>();
    }

    public static void Save(HttpContext context, Dictionary<int, int> cart)
    {
        context.Session.SetString(SessionKey, JsonSerializer.Serialize(cart));
    }

    public static void Clear(HttpContext context)
    {
        context.Session.Remove(SessionKey);
    }
}
