using ViveroApp.Dto;
using ViveroApp.DTOs;

namespace ViveroApp.Models
{
    public class HomeViewModel
    {
        public IEnumerable<MiJardinDto> MiJardin { get; set; }
        public IEnumerable<PlantaPopularDto> PlantasPopulares { get; set; }
    }
}
