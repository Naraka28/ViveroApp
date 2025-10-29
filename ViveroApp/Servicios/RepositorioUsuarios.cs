using Dapper;
using Microsoft.Data.SqlClient;
using System;
using ViveroApp.Models;

public interface IRepositorioUsuarios
{
    Task<Usuario?> ObtenerPorId(int id);
    Task<Usuario?> ObtenerPorEmail(string email);
    Task<(bool Success, int? UsuarioId)> Crear(Usuario usuario);
    Task ActualizarUltimoAcceso(int usuarioId);
    Task ActualizarPassword(int usuarioId, string passwordHash);
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
            @"INSERT INTO usuario (nombre, email, password_hash, fecha_registro, activo, created_at, updated_at)
                  VALUES (@Nombre, @Email, @PasswordHash, @FechaRegistro, @Activo, @CreatedAt, @UpdatedAt);
                  SELECT CAST(SCOPE_IDENTITY() as int);",
            usuario
        );

        return (id > 0, id);
    }

    public async Task ActualizarUltimoAcceso(int usuarioId)
    {
        using var connection = new SqlConnection(connectionString);

        await connection.ExecuteAsync(
            "UPDATE usuario SET ultimo_acceso = @UltimoAcceso WHERE id = @Id",
            new { Id = usuarioId, UltimoAcceso = DateTime.Now }
        );
    }

    public Task ActualizarPassword(int usuarioId, string passwordHash)
    {
        throw new NotImplementedException();
    }
}

