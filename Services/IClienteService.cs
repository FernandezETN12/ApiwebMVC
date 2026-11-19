using ApiwebMVC.Models;

namespace ApiwebMVC.Services;

public interface IClienteService
{
    Task<Cliente?> GetByIdAsync(int id);
    Task<Cliente?> GetByGoogleSubjectAsync(string googleSubject);
    Task<Cliente?> GetByEmailAsync(string email);
    Task<Cliente> CreateOrUpdateFromGoogleAsync(string googleSubject, string name, string email);
    Task ActualizarContactoAsync(int clienteId, string telefono, string direccion);
    Task ActualizarDireccionAsync(int clienteId, string telefono, string calle, string altura, string direccion);
}
