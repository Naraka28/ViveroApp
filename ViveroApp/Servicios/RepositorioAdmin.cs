using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using ViveroApp.Dto;
using ViveroApp.Models;
using BCrypt.Net;

namespace ViveroApp.Servicios
{
    public interface IRepositorioAdmin
    {
        Task<EstadisticasDto> ObtenerEstadisticas();

        Task<PaginacionDto<DetallePlantaDto>> ObtenerTodasPlantasPaginadas(FiltroPlantasDto filtro);
        Task<PaginacionDto<CategoriaDto>> ObtenerCategoriasPaginadas(FiltroCategoriasDto filtro);
        Task<PaginacionDto<UsuarioAdminDto>> ObtenerUsuariosPaginados(FiltroUsuariosDto filtro);
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
        Task<Riego> ObtenerRiegoPorId(int id);
        Task CrearRiego(Riego riego);
        Task ActualizarRiego(int id, Riego riego);
        Task EliminarRiego(int id);
        Task<IEnumerable<Luz>> ObtenerLuces();
        Task<Luz> ObtenerLuzPorId(int id);
        Task CrearLuz(Luz luz);
        Task ActualizarLuz(int id, Luz luz);
        Task EliminarLuz(int id);
        Task<IEnumerable<Sustrato>> ObtenerSustratos();
        Task<Sustrato> ObtenerSustratoPorId(int id);
        Task CrearSustrato(Sustrato sustrato);
        Task ActualizarSustrato(int id, Sustrato sustrato);
        Task EliminarSustrato(int id);
        Task<IEnumerable<UsuarioAdminDto>> ObtenerUsuarios();
        Task CambiarRolUsuario(int id, string nuevoRol);
        Task<EditarUsuarioDto> ObtenerUsuarioPorId(int id);
        Task CrearUsuario(CrearUsuarioDto dto);
        Task ActualizarUsuario(int id, EditarUsuarioDto dto);
        Task EliminarUsuario(int id);
        Task<IEnumerable<int>> ObtenerCategoriasPlanta(int plantaId);
        Task AsignarCategoriasPlanta(int plantaId, List<int> categoriasIds);
        Task<CategoriaDto> ObtenerCategoriaPorId(int id);
    }

    public class RepositorioAdmin : IRepositorioAdmin
    {
        private readonly string connectionString;

        public RepositorioAdmin(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<PaginacionDto<DetallePlantaDto>> ObtenerTodasPlantasPaginadas(FiltroPlantasDto filtro)
        {
            using var connection = new SqlConnection(connectionString);

            var whereClause = new StringBuilder("WHERE 1=1");
            var parametros = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            {
                whereClause.Append(" AND (nombre LIKE @Busqueda OR nombre_cientifico LIKE @Busqueda)");
                parametros.Add("Busqueda", $"%{filtro.Busqueda}%");
            }

            if (!string.IsNullOrWhiteSpace(filtro.Dificultad))
            {
                whereClause.Append(" AND dificultad = @Dificultad");
                parametros.Add("Dificultad", filtro.Dificultad);
            }

            if (filtro.Toxica.HasValue)
            {
                whereClause.Append(" AND toxica = @Toxica");
                parametros.Add("Toxica", filtro.Toxica.Value);
            }

            var sqlCount = $"SELECT COUNT(*) FROM planta {whereClause}";
            var totalRegistros = await connection.ExecuteScalarAsync<int>(sqlCount, parametros);

            var offset = (filtro.Pagina - 1) * filtro.RegistrosPorPagina;
            parametros.Add("Offset", offset);
            parametros.Add("RegistrosPorPagina", filtro.RegistrosPorPagina);

            var sqlPaginado = $@"
                SELECT * FROM planta 
                {whereClause}
                ORDER BY nombre
                OFFSET @Offset ROWS 
                FETCH NEXT @RegistrosPorPagina ROWS ONLY";

            var plantas = await connection.QueryAsync<DetallePlantaDto>(sqlPaginado, parametros);

            return new PaginacionDto<DetallePlantaDto>
            {
                Items = plantas,
                PaginaActual = filtro.Pagina,
                TotalRegistros = totalRegistros,
                RegistrosPorPagina = filtro.RegistrosPorPagina,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)filtro.RegistrosPorPagina)
            };
        }

        public async Task<PaginacionDto<CategoriaDto>> ObtenerCategoriasPaginadas(FiltroCategoriasDto filtro)
        {
            using var connection = new SqlConnection(connectionString);

            var whereClause = new StringBuilder("WHERE 1=1");
            var parametros = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            {
                whereClause.Append(" AND nombre LIKE @Busqueda");
                parametros.Add("Busqueda", $"%{filtro.Busqueda}%");
            }

            var sqlCount = $"SELECT COUNT(*) FROM categoria {whereClause}";
            var totalRegistros = await connection.ExecuteScalarAsync<int>(sqlCount, parametros);

            var offset = (filtro.Pagina - 1) * filtro.RegistrosPorPagina;
            parametros.Add("Offset", offset);
            parametros.Add("RegistrosPorPagina", filtro.RegistrosPorPagina);

            var sqlPaginado = $@"
                SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion, icono AS Icono
                FROM categoria 
                {whereClause}
                ORDER BY nombre
                OFFSET @Offset ROWS 
                FETCH NEXT @RegistrosPorPagina ROWS ONLY";

            var categorias = await connection.QueryAsync<CategoriaDto>(sqlPaginado, parametros);

            return new PaginacionDto<CategoriaDto>
            {
                Items = categorias,
                PaginaActual = filtro.Pagina,
                TotalRegistros = totalRegistros,
                RegistrosPorPagina = filtro.RegistrosPorPagina,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)filtro.RegistrosPorPagina)
            };
        }

        public async Task<PaginacionDto<UsuarioAdminDto>> ObtenerUsuariosPaginados(FiltroUsuariosDto filtro)
        {
            using var connection = new SqlConnection(connectionString);

            var whereClause = new StringBuilder("WHERE 1=1");
            var parametros = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            {
                whereClause.Append(" AND (nombre LIKE @Busqueda OR email LIKE @Busqueda)");
                parametros.Add("Busqueda", $"%{filtro.Busqueda}%");
            }

            if (!string.IsNullOrWhiteSpace(filtro.Rol))
            {
                whereClause.Append(" AND rol = @Rol");
                parametros.Add("Rol", filtro.Rol);
            }

            if (filtro.Activo.HasValue)
            {
                whereClause.Append(" AND activo = @Activo");
                parametros.Add("Activo", filtro.Activo.Value);
            }

            if (filtro.FechaRegistro.HasValue)
            {
                whereClause.Append(" AND CAST(fecha_registro AS DATE) = @FechaRegistro");
                parametros.Add("FechaRegistro", filtro.FechaRegistro.Value.Date);
            }

            var sqlCount = $"SELECT COUNT(*) FROM usuario {whereClause}";
            var totalRegistros = await connection.ExecuteScalarAsync<int>(sqlCount, parametros);

            var offset = (filtro.Pagina - 1) * filtro.RegistrosPorPagina;
            parametros.Add("Offset", offset);
            parametros.Add("RegistrosPorPagina", filtro.RegistrosPorPagina);

            var sqlPaginado = $@"
                SELECT id as Id, nombre as Nombre, email as Email, rol as Rol, 
                       activo as Activo, fecha_registro as FechaRegistro 
                FROM usuario 
                {whereClause}
                ORDER BY fecha_registro DESC
                OFFSET @Offset ROWS 
                FETCH NEXT @RegistrosPorPagina ROWS ONLY";

            var usuarios = await connection.QueryAsync<UsuarioAdminDto>(sqlPaginado, parametros);

            return new PaginacionDto<UsuarioAdminDto>
            {
                Items = usuarios,
                PaginaActual = filtro.Pagina,
                TotalRegistros = totalRegistros,
                RegistrosPorPagina = filtro.RegistrosPorPagina,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)filtro.RegistrosPorPagina)
            };
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

            string sqlDificultad = @"SELECT dificultad as Etiqueta, COUNT(*) as Valor 
                                    FROM planta 
                                        GROUP BY dificultad";
            stats.PlantasPorDificultad = (await connection.QueryAsync<DatoGrafico>(sqlDificultad)).ToList();

            string sqlUsuarios = @"SELECT FORMAT(fecha_registro, 'MM-yyyy') as Etiqueta, COUNT(*) as Valor 
                                   FROM usuario 
                                   WHERE fecha_registro >= DATEADD(MONTH, -6, GETDATE())
                                   GROUP BY FORMAT(fecha_registro, 'MM-yyyy'), YEAR(fecha_registro), MONTH(fecha_registro)
                                   ORDER BY YEAR(fecha_registro), MONTH(fecha_registro)";
            stats.UsuariosUltimos6Meses = (await connection.QueryAsync<DatoGrafico>(sqlUsuarios)).ToList();

            string sqlCategorias = @"SELECT TOP 5 c.nombre as Etiqueta, COUNT(pc.planta_id) as Valor
                                    FROM categoria c
                                    LEFT JOIN planta_categoria pc ON c.id = pc.categoria_id
                                    GROUP BY c.id, c.nombre
                                    ORDER BY Valor DESC";
            stats.TopCategorias = (await connection.QueryAsync<DatoGrafico>(sqlCategorias)).ToList();

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
                SELECT id AS Id, nombre AS Nombre, nombre_cientifico AS NombreCientifico,
                       descripcion AS Descripcion, imagen_url AS ImagenUrl, riego_id AS RiegoId,
                       luz_id AS LuzId, sustrato_id AS SustratoId, cuidados_especiales AS CuidadosEspeciales,
                       altura_max_cm AS AlturaMaxCm, dificultad AS Dificultad, toxica AS Toxica, notas AS Notas
                FROM planta WHERE id = @id";

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
                UPDATE planta SET nombre = @Nombre, nombre_cientifico = @NombreCientifico, 
                       descripcion = @Descripcion, imagen_url = @ImagenUrl, riego_id = @RiegoId, 
                       luz_id = @LuzId, sustrato_id = @SustratoId, cuidados_especiales = @CuidadosEspeciales, 
                       altura_max_cm = @AlturaMaxCm, dificultad = @Dificultad, toxica = @Toxica, 
                       notas = @Notas, updated_at = GETDATE()
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

        public async Task<IEnumerable<Riego>> ObtenerRiegos()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Riego>("SELECT * FROM riego ORDER BY nombre");
        }

        public async Task<Riego> ObtenerRiegoPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Riego>("SELECT * FROM riego WHERE id = @id", new { id });
        }

        public async Task CrearRiego(Riego riego)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                INSERT INTO riego (nombre, descripcion, frecuencia_dias_verano, frecuencia_dias_invierno, nivel, recomendaciones)
                VALUES (@Nombre, @Descripcion, @FrecuenciaDiasVerano, @FrecuenciaDiasInvierno, @Nivel, @Recomendaciones)", riego);
        }

        public async Task ActualizarRiego(int id, Riego riego)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                UPDATE riego SET nombre = @Nombre, descripcion = @Descripcion,
                       frecuencia_dias_verano = @FrecuenciaDiasVerano, frecuencia_dias_invierno = @FrecuenciaDiasInvierno,
                       nivel = @Nivel, recomendaciones = @Recomendaciones
                WHERE id = @Id", new { Id = id, riego.Nombre, riego.Descripcion, riego.FrecuenciaDiasVerano, riego.FrecuenciaDiasInvierno, riego.Nivel, riego.Recomendaciones });
        }

        public async Task EliminarRiego(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM riego WHERE id = @id", new { id });
        }

        public async Task<IEnumerable<Luz>> ObtenerLuces()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Luz>("SELECT * FROM luz ORDER BY tipo");
        }

        public async Task<Luz> ObtenerLuzPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Luz>("SELECT * FROM luz WHERE id = @id", new { id });
        }

        public async Task CrearLuz(Luz luz)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                INSERT INTO luz (tipo, descripcion, horas_min, horas_max, ubicacion_recomendada)
                VALUES (@Tipo, @Descripcion, @HorasMin, @HorasMax, @UbicacionRecomendada)", luz);
        }

        public async Task ActualizarLuz(int id, Luz luz)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                UPDATE luz SET tipo = @Tipo, descripcion = @Descripcion, horas_min = @HorasMin,
                       horas_max = @HorasMax, ubicacion_recomendada = @UbicacionRecomendada
                WHERE id = @Id", new { Id = id, luz.Tipo, luz.Descripcion, luz.HorasMin, luz.HorasMax, luz.UbicacionRecomendada });
        }

        public async Task EliminarLuz(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM luz WHERE id = @id", new { id });
        }

        public async Task<IEnumerable<Sustrato>> ObtenerSustratos()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Sustrato>("SELECT * FROM sustrato ORDER BY tipo");
        }

        public async Task<Sustrato> ObtenerSustratoPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Sustrato>("SELECT * FROM sustrato WHERE id = @id", new { id });
        }

        public async Task CrearSustrato(Sustrato sustrato)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                INSERT INTO sustrato (tipo, descripcion, ph_rango, drenaje, composicion, recomendaciones)
                VALUES (@Tipo, @Descripcion, @PhRango, @Drenaje, @Composicion, @Recomendaciones)", sustrato);
        }

        public async Task ActualizarSustrato(int id, Sustrato sustrato)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                UPDATE sustrato SET tipo = @Tipo, descripcion = @Descripcion, ph_rango = @PhRango,
                       drenaje = @Drenaje, composicion = @Composicion, recomendaciones = @Recomendaciones
                WHERE id = @Id", new { Id = id, sustrato.Tipo, sustrato.Descripcion, sustrato.PhRango, sustrato.Drenaje, sustrato.Composicion, sustrato.Recomendaciones });
        }

        public async Task EliminarSustrato(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM sustrato WHERE id = @id", new { id });
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
            await connection.ExecuteAsync("UPDATE usuario SET rol = @nuevoRol WHERE id = @id", new { id, nuevoRol });
        }

        public async Task<EditarUsuarioDto> ObtenerUsuarioPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<EditarUsuarioDto>("SELECT Id, Nombre, Email, Rol, Activo FROM usuario WHERE id = @id", new { id });
        }

        public async Task CrearUsuario(CrearUsuarioDto dto)
        {
            using var connection = new SqlConnection(connectionString);
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await connection.ExecuteAsync(@"
                INSERT INTO usuario (nombre, email, password, rol, activo, fecha_registro, created_at, updated_at)
                VALUES (@Nombre, @Email, @PasswordHash, @Rol, @Activo, GETDATE(), GETDATE(), GETDATE())",
                new { dto.Nombre, dto.Email, PasswordHash = passwordHash, dto.Rol, dto.Activo });
        }

        public async Task ActualizarUsuario(int id, EditarUsuarioDto dto)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                UPDATE usuario SET nombre = @Nombre, email = @Email, rol = @Rol, activo = @Activo, updated_at = GETDATE()
                WHERE id = @Id", new { Id = id, dto.Nombre, dto.Email, dto.Rol, dto.Activo });
        }

        public async Task EliminarUsuario(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM usuario WHERE id = @id", new { id });
        }

        public async Task<IEnumerable<int>> ObtenerCategoriasPlanta(int plantaId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<int>("SELECT categoria_id FROM planta_categoria WHERE planta_id = @plantaId", new { plantaId });
        }

        public async Task AsignarCategoriasPlanta(int plantaId, List<int> categoriasIds)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM planta_categoria WHERE planta_id = @plantaId", new { plantaId });

            if (categoriasIds != null && categoriasIds.Any())
            {
                foreach (var categoriaId in categoriasIds)
                {
                    await connection.ExecuteAsync("INSERT INTO planta_categoria (planta_id, categoria_id) VALUES (@plantaId, @categoriaId)", new { plantaId, categoriaId });
                }
            }
        }

        public async Task<CategoriaDto> ObtenerCategoriaPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            string sql = @"SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion, icono AS Icono FROM categoria WHERE id = @id";
            return await connection.QueryFirstOrDefaultAsync<CategoriaDto>(sql, new { id });
        }

        public async Task<IEnumerable<CategoriaDto>> ObtenerCategorias()
        {
            using var connection = new SqlConnection(connectionString);
            string sql = @"SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion, icono AS Icono FROM categoria ORDER BY nombre";
            return await connection.QueryAsync<CategoriaDto>(sql);
        }

        public async Task CrearCategoria(CategoriaDto dto)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"INSERT INTO categoria (nombre, descripcion, icono) VALUES (@Nombre, @Descripcion, @Icono)", dto);
        }

        public async Task ActualizarCategoria(int id, CategoriaDto dto)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE categoria SET nombre = @Nombre, descripcion = @Descripcion, icono = @Icono WHERE id = @Id",
                new { Id = id, dto.Nombre, dto.Descripcion, dto.Icono });
        }

        public async Task EliminarCategoria(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM categoria WHERE id = @id", new { id });
        }
    }
}