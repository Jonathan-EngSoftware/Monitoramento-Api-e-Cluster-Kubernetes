using CadastroApi.Models;
using CadastroApi.Repositories;

namespace CadastroApi.Services;

public class UsuarioService
{
    private readonly UsuarioRepository _repository;

    public UsuarioService(UsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<Usuario> CriarAsync(string nome, string email)
    {
        var usuario = new Usuario
        {
            Nome = nome,
            Email = email
        };

        return await _repository.CriarAsync(usuario);
    }

    public async Task<List<Usuario>> ListarAsync()
    {
        return await _repository.ListarAsync();
    }
}