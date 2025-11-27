using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using ViveroApp.Dto;
using ViveroApp.Models;

namespace ViveroApp.Servicios
{
    public interface IRepositorioPlantas
    {
        Task<IEnumerable<Plantas>> ObtenerTodasLasPlantas();
        Task<IEnumerable<PlantaPopularDto>> ObtenerPlantasPopulares(int top = 10);
        Task<DetallePlantaDto> ObtenerDetalle(int id);
        Task<IEnumerable<BusquedaPlantaDto>> BuscarPlantas(string termino);
        Task<PlantasPorCategoriaDto> ObtenerPlantasPorCategoria(string categoria);
        Task<IEnumerable<PlantaPopularDto>> ObtenerPlantasPorRecomendacion(string tipo);
    }

    public class RepositorioPlantas : IRepositorioPlantas
    {
        private readonly string connectionString;

        public RepositorioPlantas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Plantas>> ObtenerTodasLasPlantas()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Plantas>("SELECT * FROM planta");
        }

        public async Task<DetallePlantaDto> ObtenerDetalle(int plantaId)
        {
            using var connection = new SqlConnection(connectionString);
            using var multi = await connection.QueryMultipleAsync(
                "sp_detalle_planta",
                new { planta_id = plantaId },
                commandType: CommandType.StoredProcedure
            );

            var planta = await multi.ReadFirstOrDefaultAsync<DetallePlantaDto>();
            if (planta == null) return null;

            var categorias = await multi.ReadAsync<CategoriaDto>();
            planta.Categorias = categorias.ToList();

            return planta;
        }

        public async Task<IEnumerable<PlantaPopularDto>> ObtenerPlantasPopulares(int top = 10)
        {
            using var connection = new SqlConnection(connectionString);
            var plantas = await connection.QueryAsync<PlantaPopularDto>(
                "sp_plantas_populares",
                new { top },
                commandType: CommandType.StoredProcedure
            );
            return plantas;
        }

        public async Task<IEnumerable<BusquedaPlantaDto>> BuscarPlantas(string termino)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"
                SELECT TOP 10 
                    Id,
                    Nombre,
                    Nombre_Cientifico,
                    Imagen_Url,
                    Dificultad
                FROM planta
                WHERE Nombre LIKE @Termino 
                   OR Nombre_Cientifico LIKE @Termino
                ORDER BY 
                    CASE 
                        WHEN Nombre LIKE @TerminoExacto THEN 1
                        WHEN Nombre_Cientifico LIKE @TerminoExacto THEN 2
                        ELSE 3
                    END,
                    Nombre";

            return await connection.QueryAsync<BusquedaPlantaDto>(query, new
            {
                Termino = $"%{termino}%",
                TerminoExacto = $"{termino}%"
            });
        }
            
        public async Task<PlantasPorCategoriaDto> ObtenerPlantasPorCategoria(string categoria)
        {
            using var connection = new SqlConnection(connectionString);

            // Obtener descripción de la categoría
            var categoriaInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT Nombre, Descripcion FROM categoria WHERE Nombre = @categoria",
                new { categoria }
            );

            if (categoriaInfo == null)
            {
                return null;
            }

            // Obtener plantitas
            var plantas = await connection.QueryAsync<PlantaPopularDto>(
                "sp_plantas_por_categoria",
                new { categoria },
                commandType: CommandType.StoredProcedure
            );

            return new PlantasPorCategoriaDto
            {
                CategoriaNombre = categoriaInfo.Nombre,
                CategoriaDescripcion = categoriaInfo.Descripcion ?? "Descubre nuestra colección",
                Plantas = plantas
            };
        }
        public async Task<IEnumerable<PlantaPopularDto>> ObtenerPlantasPorRecomendacion(string tipo)
        {
            using var connection = new SqlConnection(connectionString);

            string whereClause = tipo switch
            {
                "Principiantes" => "WHERE p.dificultad = 'baja'",
                "PocaLuz" => "WHERE l.tipo IN ('Sol Parcial', 'Sombra Parcial', 'Sombra')",
                "BajoRiego" => "WHERE r.nombre IN ('Riego Escaso', 'Riego Suculentas')",
                _ => string.Empty
            };

            string query = $@"
            SELECT p.id as Id, p.nombre, p.nombre_cientifico as NombreCientifico, 
            p.imagen_url as ImagenUrl, p.dificultad,
            COUNT(mj.id) as UsuariosQueLaTienen,    
            r.nombre as TipoRiego, l.tipo as TipoLuz 
            FROM planta p
            LEFT JOIN riego r ON p.riego_id = r.id
            LEFT JOIN luz l ON p.luz_id = l.id              
            LEFT JOIN mi_Jardin mj ON p.id = mj.planta_id       
            {whereClause}
     
            GROUP BY p.id, p.nombre, p.nombre_cientifico, p.imagen_url, 
              p.dificultad, r.nombre, l.tipo             
            ORDER BY p.nombre";

            var plantas = await connection.QueryAsync<PlantaPopularDto>(query);
            return plantas;
        }
    }
}