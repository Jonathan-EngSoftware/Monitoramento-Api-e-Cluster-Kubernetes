using Microsoft.AspNetCore.Mvc;
using CadastroApi.Services;
using CadastroApi.DTOs;

namespace CadastroApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly UsuarioService _service;

    public UsuarioController(UsuarioService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] UsuarioDTO dto)
    {
        var usuario = await _service.CriarAsync(dto.Nome, dto.Email);
        return Ok(usuario);
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var usuarios = await _service.ListarAsync();
        return Ok(usuarios);
    }
}