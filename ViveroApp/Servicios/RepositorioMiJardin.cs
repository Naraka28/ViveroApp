using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using ViveroApp.DTOs;


namespace ViveroApp.Servicios
{
    public interface IRepositorioMiJardin
    {
        Task<IEnumerable<MiJardinDto>> ObtenerPlantasUsuario(int usuarioId);
        Task<int> AgregarPlanta(int usuarioId, int plantaId, string? ubicacion, string? apodo);
        Task<bool> EliminarPlanta(int miJardinId, int usuarioId);
        Task<bool> ActualizarPlanta(int miJardinId, int usuarioId, string? ubicacion, string? apodo);
        Task<bool> RegistrarRiego(int miJardinId, int usuarioId);
        Task Ordenar(string ids, int usuarioId);
    }
    public class RepositorioMiJardin : IRepositorioMiJardin
    {
        private readonly string connectionString;

        public RepositorioMiJardin(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<MiJardinDto>> ObtenerPlantasUsuario(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            var plantas = await connection.QueryAsync<MiJardinDto>(
                "sp_obtener_mi_jardin",
                new { usuario_id = usuarioId },
                commandType: CommandType.StoredProcedure
            );

            return plantas;
        }

        public async Task<int> AgregarPlanta(int usuarioId, int plantaId, string? ubicacion, string? apodo)
        {
            using var connection = new SqlConnection(connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@usuario_id", usuarioId);
            parameters.Add("@planta_id", plantaId);
            parameters.Add("@ubicacion", ubicacion);
            parameters.Add("@apodo", apodo);
            parameters.Add("@mi_jardin_id", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_agregar_planta_mi_jardin",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return parameters.Get<int>("@mi_jardin_id");
        }

        public async Task<bool> EliminarPlanta(int miJardinId, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            try
            {
                await connection.ExecuteAsync(
                    "sp_eliminar_planta_mi_jardin",
                    new { mi_jardin_id = miJardinId, usuario_id = usuarioId },
                    commandType: CommandType.StoredProcedure
                );

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActualizarPlanta(int miJardinId, int usuarioId, string? ubicacion, string? apodo)
        {
            using var connection = new SqlConnection(connectionString);

            try
            {
                await connection.ExecuteAsync(
                    "sp_actualizar_planta_mi_jardin",
                    new
                    {
                        mi_jardin_id = miJardinId,
                        usuario_id = usuarioId,
                        ubicacion = ubicacion,
                        apodo = apodo,
                        notas_personales = (string?)null,
                        fecha_adquisicion = (DateTime?)null,
                        ultimo_riego = (DateTime?)null,
                        ultimo_fertilizado = (DateTime?)null
                    },
                    commandType: CommandType.StoredProcedure
                );

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RegistrarRiego(int miJardinId, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            try
            {
                await connection.ExecuteAsync(
                    "sp_actualizar_ultimo_riego",
                    new { mi_jardin_id = miJardinId, usuario_id = usuarioId, fecha_riego = (DateTime?)null },
                    commandType: CommandType.StoredProcedure
                );

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task Ordenar(string ids, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(
                "sp_ordenar_mi_jardin",
                new { ids = ids, usuario_id = usuarioId },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}