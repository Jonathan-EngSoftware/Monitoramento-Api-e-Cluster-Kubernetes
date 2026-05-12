using CadastroApi.Data;
using CadastroApi.Models;

namespace CadastroApi.Repositories;

public class UsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario> CriarAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<List<Usuario>> ListarAsync()
    {
        return _context.Usuarios.ToList();
    }
}