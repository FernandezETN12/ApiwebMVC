using ApiwebMVC.Models;

namespace ApiwebMVC.Services;

public interface IClienteService
{
    Task<Cliente?> GetByGoogleSubjectAsync(string googleSubject);
    Task<Cliente?> GetByEmailAsync(string email);
    Task<Cliente> CreateOrUpdateFromGoogleAsync(string googleSubject, string name, string email);
}
