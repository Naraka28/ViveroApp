using Dapper;
using Microsoft.AspNetCore.Mvc;
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
            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsync<Plantas>("SELECT * FROM planta");
            }
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
    }
}
