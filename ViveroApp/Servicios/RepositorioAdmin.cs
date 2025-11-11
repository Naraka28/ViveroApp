using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using ViveroApp.Dto;
using ViveroApp.Models;
using BCrypt.Net;

namespace ViveroApp.Servicios
{
    public interface IRepositorioAdmin
    {
        Task<EstadisticasDto> ObtenerEstadisticas();
        Task<IEnumerable<DetallePlantaDto>> ObtenerTodasPlantas();
        Task<EditarPlantaDto> ObtenerPlantaPorId(int id);
        Task CrearPlanta(CrearPlantaDto dto);
        Task ActualizarPlanta(int id, EditarPlantaDto dto);
        Task EliminarPlanta(int id);
        Task<IEnumerable<CategoriaDto>> ObtenerCategorias();
        Task CrearCategoria(CategoriaDto dto);
        Task ActualizarCategoria(int id, CategoriaDto dto);
        Task EliminarCategoria(int id);

        Task<IEnumerable<Riego>> ObtenerRiegos();
        Task<IEnumerable<Luz>> ObtenerLuces();
        Task<IEnumerable<Sustrato>> ObtenerSustratos();

        Task<IEnumerable<UsuarioAdminDto>> ObtenerUsuarios();
        Task CambiarRolUsuario(int id, string nuevoRol);

        Task<EditarUsuarioDto> ObtenerUsuarioPorId(int id);
        Task CrearUsuario(CrearUsuarioDto dto);
        Task ActualizarUsuario(int id, EditarUsuarioDto dto);
        Task EliminarUsuario(int id);
        Task<IEnumerable<int>> ObtenerCategoriasPlanta(int plantaId);
        Task AsignarCategoriasPlanta(int plantaId, List<int> categoriasIds);
    }
    public class RepositorioAdmin : IRepositorioAdmin
    {
        private readonly string connectionString;

        public RepositorioAdmin(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<EstadisticasDto> ObtenerEstadisticas()
        {
            using var connection = new SqlConnection(connectionString);

            var stats = new EstadisticasDto
            {
                TotalPlantas = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM planta"),
                TotalUsuarios = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM usuario"),
                TotalCategorias = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM categoria"),
                PlantasEnJardines = await connection.ExecuteScalarAsync<int>("SELECT COUNT(DISTINCT planta_id) FROM mi_jardin")
            };

            return stats;
        }

        public async Task<IEnumerable<DetallePlantaDto>> ObtenerTodasPlantas()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<DetallePlantaDto>("SELECT * FROM planta ORDER BY nombre");
        }

        public async Task<EditarPlantaDto> ObtenerPlantaPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);

            string sql = @"
        SELECT 
            id AS Id,
            nombre AS Nombre,
            nombre_cientifico AS NombreCientifico,
            descripcion AS Descripcion,
            imagen_url AS ImagenUrl,
            riego_id AS RiegoId,
            luz_id AS LuzId,
            sustrato_id AS SustratoId,
            cuidados_especiales AS CuidadosEspeciales,
            altura_max_cm AS AlturaMaxCm,
            dificultad AS Dificultad,
            toxica AS Toxica,
            notas AS Notas
        FROM planta 
        WHERE id = @id";

            var planta = await connection.QueryFirstOrDefaultAsync<EditarPlantaDto>(sql, new { id });

            if (planta != null)
            {
                var categoriasIds = await ObtenerCategoriasPlanta(id);
                planta.CategoriasIds = categoriasIds.ToList();
            }

            return planta;
        }

        public async Task CrearPlanta(CrearPlantaDto dto)
        {
            using var connection = new SqlConnection(connectionString);

            var plantaId = await connection.QuerySingleAsync<int>(@"
        INSERT INTO planta (nombre, nombre_cientifico, descripcion, imagen_url, riego_id, luz_id, sustrato_id, 
                            cuidados_especiales, altura_max_cm, dificultad, toxica, notas)
        OUTPUT INSERTED.id
        VALUES (@Nombre, @NombreCientifico, @Descripcion, @ImagenUrl, @RiegoId, @LuzId, @SustratoId, 
                @CuidadosEspeciales, @AlturaMaxCm, @Dificultad, @Toxica, @Notas)", dto);

            await AsignarCategoriasPlanta(plantaId, dto.CategoriasIds);
        }

        public async Task ActualizarPlanta(int id, EditarPlantaDto dto)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(@"
        UPDATE planta SET 
            nombre = @Nombre, 
            nombre_cientifico = @NombreCientifico, 
            descripcion = @Descripcion, 
            imagen_url = @ImagenUrl,
            riego_id = @RiegoId, 
            luz_id = @LuzId, 
            sustrato_id = @SustratoId,
            cuidados_especiales = @CuidadosEspeciales, 
            altura_max_cm = @AlturaMaxCm, 
            dificultad = @Dificultad, 
            toxica = @Toxica, 
            notas = @Notas,
            updated_at = GETDATE()
        WHERE id = @Id", new
            {
                Id = id,
                dto.Nombre,
                dto.NombreCientifico,
                dto.Descripcion,
                dto.ImagenUrl,
                dto.RiegoId,
                dto.LuzId,
                dto.SustratoId,
                dto.CuidadosEspeciales,
                dto.AlturaMaxCm,
                dto.Dificultad,
                dto.Toxica,
                dto.Notas
            });

            await AsignarCategoriasPlanta(id, dto.CategoriasIds);
        }

        public async Task EliminarPlanta(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM planta WHERE id = @id", new { id });
        }

        public async Task<IEnumerable<CategoriaDto>> ObtenerCategorias()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<CategoriaDto>("SELECT * FROM categoria ORDER BY nombre");
        }

        public async Task CrearCategoria(CategoriaDto dto)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "INSERT INTO categoria (nombre, descripcion, icono) VALUES (@Nombre, @Descripcion, @Icono)", dto);
        }

        public async Task ActualizarCategoria(int id, CategoriaDto dto)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                UPDATE categoria SET nombre = @Nombre, descripcion = @Descripcion, icono = @Icono 
                WHERE id = @Id", new { Id = id, dto.Nombre, dto.Descripcion, dto.Icono });
        }

        public async Task EliminarCategoria(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM categoria WHERE id = @id", new { id });
        }

        public async Task<IEnumerable<Riego>> ObtenerRiegos()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Riego>("SELECT * FROM riego ORDER BY nombre");
        }

        public async Task<IEnumerable<Luz>> ObtenerLuces()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Luz>("SELECT * FROM luz ORDER BY tipo");
        }

        public async Task<IEnumerable<Sustrato>> ObtenerSustratos()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Sustrato>("SELECT * FROM sustrato ORDER BY tipo");
        }

        public async Task<IEnumerable<UsuarioAdminDto>> ObtenerUsuarios()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<UsuarioAdminDto>(@"
                SELECT id as Id, nombre as Nombre, email as Email, rol as Rol, 
                       activo as Activo, fecha_registro as FechaRegistro 
                FROM usuario ORDER BY fecha_registro DESC");
        }

        public async Task CambiarRolUsuario(int id, string nuevoRol)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "UPDATE usuario SET rol = @nuevoRol WHERE id = @id",
                new { id, nuevoRol });
        }

        public async Task<EditarUsuarioDto> ObtenerUsuarioPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<EditarUsuarioDto>(
                "SELECT Id, Nombre, Email, Rol, Activo FROM usuario WHERE id = @id", new { id });
        }

        public async Task CrearUsuario(CrearUsuarioDto dto)
        {
            using var connection = new SqlConnection(connectionString);

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await connection.ExecuteAsync(@"
        INSERT INTO usuario (Nombre, Email, Password, Rol, Activo, FechaRegistro, CreatedAt, UpdatedAt)
        VALUES (@Nombre, @Email, @PasswordHash, @Rol, @Activo, GETDATE(), GETDATE(), GETDATE())",
            new
            {
                dto.Nombre,
                dto.Email,
                PasswordHash = passwordHash,
                dto.Rol,
                dto.Activo
            });
        }

        public async Task ActualizarUsuario(int id, EditarUsuarioDto dto)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
        UPDATE usuario SET 
            Nombre = @Nombre, 
            Email = @Email, 
            Rol = @Rol, 
            Activo = @Activo,
            UpdatedAt = GETDATE()
        WHERE id = @Id", new
            {
                Id = id,
                dto.Nombre,
                dto.Email,
                dto.Rol,
                dto.Activo
            });
        }

        public async Task EliminarUsuario(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM usuario WHERE id = @id", new { id });
        }

        public async Task<IEnumerable<int>> ObtenerCategoriasPlanta(int plantaId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<int>(
                "SELECT categoria_id FROM planta_categoria WHERE planta_id = @plantaId",
                new { plantaId });
        }

        public async Task AsignarCategoriasPlanta(int plantaId, List<int> categoriasIds)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(
                "DELETE FROM planta_categoria WHERE planta_id = @plantaId",
                new { plantaId });

            if (categoriasIds != null && categoriasIds.Any())
            {
                foreach (var categoriaId in categoriasIds)
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO planta_categoria (planta_id, categoria_id) VALUES (@plantaId, @categoriaId)",
                        new { plantaId, categoriaId });
                }
            }
        }
    }
}