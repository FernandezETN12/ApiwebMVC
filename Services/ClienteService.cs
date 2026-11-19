using ApiwebMVC.Data;
using ApiwebMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiwebMVC.Services;

public sealed class ClienteService : IClienteService
{
    private readonly ApplicationDbContext _db;

    public ClienteService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Cliente?> GetByIdAsync(int id) =>
        await _db.Clientes.FindAsync(id);

    public async Task<Cliente?> GetByGoogleSubjectAsync(string googleSubject) =>
        await _db.Clientes.FirstOrDefaultAsync(c => c.GoogleSubject == googleSubject);

    public async Task<Cliente?> GetByEmailAsync(string email) =>
        await _db.Clientes.FirstOrDefaultAsync(c => c.Email == email);

    public async Task<Cliente> CreateOrUpdateFromGoogleAsync(string googleSubject, string name, string email)
    {
        var cliente = await GetByGoogleSubjectAsync(googleSubject);

        if (cliente is null)
        {
            cliente = await GetByEmailAsync(email);
        }

        if (cliente is null)
        {
            cliente = new Cliente
            {
                GoogleSubject = googleSubject,
                Nombre = name,
                Email = email,
                FechaRegistro = DateTime.UtcNow
            };
            _db.Clientes.Add(cliente);
        }
        else
        {
            cliente.GoogleSubject = googleSubject;
            cliente.Nombre = name;
            cliente.Email = email;
        }

        await _db.SaveChangesAsync();
        return cliente;
    }

    public async Task ActualizarContactoAsync(int clienteId, string telefono, string direccion)
    {
        var cliente = await _db.Clientes.FindAsync(clienteId)
            ?? throw new Exceptions.NotFoundException("El cliente indicado no existe.");
        cliente.Telefono = telefono;
        cliente.Direccion = direccion;
        await _db.SaveChangesAsync();
    }

    public async Task ActualizarDireccionAsync(int clienteId, string telefono, string calle, string altura, string direccion)
    {
        var cliente = await _db.Clientes.FindAsync(clienteId)
            ?? throw new Exceptions.NotFoundException("El cliente indicado no existe.");
        cliente.Telefono = telefono;
        cliente.Calle = calle;
        cliente.Altura = altura;
        cliente.Direccion = direccion;
        await _db.SaveChangesAsync();
    }
}
