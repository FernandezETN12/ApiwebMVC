using System.Net;
using System.Net.Http.Json;
using ApiwebMVC.Models;
using ApiwebMVC.Models.DTOs;

namespace ApiwebMVC.Services;

public sealed class PedidoApiClient : IPedidoApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PedidoApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyCollection<Pedido>> GetByClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            () => _httpClient.GetAsync(GetUrl($"api/pedidos/cliente/{clienteId}"), cancellationToken));

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Debe iniciar sesión para consultar pedidos.");

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<List<Pedido>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<Pedido?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            () => _httpClient.GetAsync(GetUrl($"api/pedidos/{id}"), cancellationToken));

        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
            return null;

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<Pedido>(cancellationToken: cancellationToken);
    }

    public async Task<Pedido> CreateAsync(CreatePedidoRequest request, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            () => _httpClient.PostAsJsonAsync(GetUrl("api/pedidos"), request, cancellationToken));

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<Pedido>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("La API no devolvió el pedido creado.");
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            () => _httpClient.DeleteAsync(GetUrl($"api/pedidos/{id}"), cancellationToken));

        await EnsureSuccessAsync(response);
    }

    private async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        var context = _httpContextAccessor.HttpContext;
        var cookie = context?.Request.Headers.Cookie.ToString();

        if (!string.IsNullOrWhiteSpace(cookie))
        {
            _httpClient.DefaultRequestHeaders.Remove("Cookie");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", cookie);
        }

        try
        {
            return await send();
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Remove("Cookie");
        }
    }

    private string GetUrl(string path)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
            throw new InvalidOperationException("No hay una petición HTTP activa para consumir la API.");

        return $"{request.Scheme}://{request.Host}/{path}";
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var detail = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(
            $"La API REST respondió {(int)response.StatusCode} ({response.ReasonPhrase}). {detail}",
            null,
            response.StatusCode);
    }
}
