using CadastroApi.Data;
using CadastroApi.Repositories;
using CadastroApi.Services;
using Microsoft.EntityFrameworkCore;
using Prometheus; // <-- 1. IMPORTAÇÃO OBRIGATÓRIA AQUI

var builder = WebApplication.CreateBuilder(args);

// MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    ));

// 
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// <-- 2. ORDEM CORRETA DO PIPELINE HTTP -->
app.UseRouting(); // Adiciona o roteamento explicitamente
app.UseHttpMetrics(); // Coleta as métricas ANTES de chegar no Controller

app.MapControllers(); // Mapeia as suas rotas da API
app.MapMetrics(); // Cria a rota /metrics para o Prometheus ler os dados

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated(); // Cria o banco e as tabelas se não existirem
}

app.Run();