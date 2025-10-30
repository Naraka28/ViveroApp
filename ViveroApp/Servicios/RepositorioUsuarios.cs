using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using ViveroApp.Models;

public interface IRepositorioUsuarios
{
    Task<Usuario?> ObtenerPorId(int id);
    Task<Usuario?> ObtenerPorEmail(string email);
    Task<(bool Success, int? UsuarioId)> Crear(Usuario usuario);
    Task ActualizarUltimoAcceso(int usuarioId);
    Task ActualizarPassword(int usuarioId, string password);
}
public class RepositorioUsuarios : IRepositorioUsuarios
{
    private readonly string connectionString;

    public RepositorioUsuarios(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }
    public async Task<Usuario?> ObtenerPorId(int id)
    {
        using var connection = new SqlConnection(connectionString);

        var usuario = await connection.QueryFirstOrDefaultAsync<Usuario>(
            "SELECT * FROM usuario WHERE id = @Id",
            new { Id = id }
        );

        return usuario;
    }

    public async Task<Usuario?> ObtenerPorEmail(string email)
    {
        using var connection = new SqlConnection(connectionString);

        var usuario = await connection.QueryFirstOrDefaultAsync<Usuario>(
            "SELECT * FROM usuario WHERE email = @Email",
            new { Email = email }
        );

        return usuario;
    }

    public async Task<(bool Success, int? UsuarioId)> Crear(Usuario usuario)
    {
        using var connection = new SqlConnection(connectionString);

        var id = await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO usuario (nombre, email, password, fecha_registro, activo, created_at, updated_at)
                  VALUES (@Nombre, @Email, @Password, @FechaRegistro, @Activo, @CreatedAt, @UpdatedAt);
                  SELECT CAST(SCOPE_IDENTITY() as int);",
            usuario
        );

        return (id > 0, id);
    }

    public async Task ActualizarUltimoAcceso(int usuarioId)
    {
        using var connection = new SqlConnection(connectionString);
        
        await connection.ExecuteAsync(
            "sp_actualizar_ultimo_acceso",
            new { usuario_id = usuarioId },
            commandType: CommandType.StoredProcedure
        );
    }

    public Task ActualizarPassword(int usuarioId, string password)
    {
        throw new NotImplementedException();
    }
}

