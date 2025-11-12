using Dapper;
using Microsoft.AspNetCore.Identity;
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
    Task ActualizarPerfil(int usuarioId, string nombre, string email);
    Task DesactivarCuenta(int usuarioId);
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
     @"SELECT 
        id,
        nombre,
        email,
        password,
        fecha_registro AS FechaRegistro,
        ultimo_acceso AS UltimoAcceso,
        activo,
        created_at AS CreatedAt,
        updated_at AS UpdatedAt,
        rol
     FROM usuario
     WHERE id = @Id",
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

    public async Task ActualizarPassword(int usuarioId, string password)
    {
        using var connection = new SqlConnection(connectionString);

        await connection.ExecuteAsync(
            "UPDATE usuario SET password = @PasswordHash, updated_at = @UpdatedAt WHERE id = @Id",
            new { Id = usuarioId, PasswordHash = password, UpdatedAt = DateTime.Now }
        );
    }
    public async Task ActualizarPerfil(int usuarioId, string nombre, string email)
    {
        using var connection = new SqlConnection(connectionString);

        // Verificar que el email no esté en uso por otro usuario
        var emailEnUso = await connection.QueryFirstOrDefaultAsync<int?>(
            "SELECT id FROM usuario WHERE email = @Email AND id != @UsuarioId",
            new { Email = email, UsuarioId = usuarioId }
        );

        if (emailEnUso.HasValue)
            throw new Exception("El email ya está en uso por otro usuario");

        await connection.ExecuteAsync(
            @"UPDATE usuario 
                  SET nombre = @Nombre, 
                      email = @Email, 
                      updated_at = GETDATE() 
                  WHERE id = @UsuarioId",
            new { UsuarioId = usuarioId, Nombre = nombre, Email = email }
        );
    }
    public async Task DesactivarCuenta(int usuarioId)
    {
        using var connection = new SqlConnection(connectionString);

        await connection.ExecuteAsync(
            "UPDATE usuario SET activo = 0, updated_at = GETDATE() WHERE id = @UsuarioId",
            new { UsuarioId = usuarioId }
        );
    }
}

