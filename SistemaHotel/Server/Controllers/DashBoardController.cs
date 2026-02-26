using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SistemaHotel.Server.Repositorio.Contratos;
using SistemaHotel.Shared;

namespace SistemaHotel.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoardController : ControllerBase
    {
        private readonly IDashBoardRepositorio _dashboardRepositorio;
        private readonly IMapper _mapper;

        public DashBoardController(IDashBoardRepositorio dashboardRepositorio, IMapper mapper)
        {
            _dashboardRepositorio = dashboardRepositorio;
            _mapper = mapper;
        }

        // RESUMEN GENERAL
        [HttpGet("Resumen")]
        public async Task<IActionResult> Resumen()
        {
            var dto = new DashBoardDTO();

            dto.TotalHabitaciones = await _dashboardRepositorio.TotalHabitaciones();
            dto.TotalHabitacionesDisponibles = await _dashboardRepositorio.HabitacionesDisponibles();
            dto.TotalHabitacionesOcupadas = await _dashboardRepositorio.HabitacionesOcupadas();
            dto.TotalHabitacionesEnLimpieza = await _dashboardRepositorio.HabitacionesLimpieza();
            dto.TotalReservasHoy = await _dashboardRepositorio.TotalReservasHoy();
            dto.TotalReservasMes = await _dashboardRepositorio.TotalReservasMes();

            return Ok(dto);
        }

        // INDIVIDUALES

        [HttpGet("TotalHabitaciones")]
        public async Task<IActionResult> TotalHabitaciones()
            => Ok(await _dashboardRepositorio.TotalHabitaciones());

        [HttpGet("TotalHabitacionesDisponibles")]
        public async Task<IActionResult> TotalHabitacionesDisponibles()
            => Ok(await _dashboardRepositorio.HabitacionesDisponibles());

        [HttpGet("TotalHabitacionesOcupadas")]
        public async Task<IActionResult> TotalHabitacionesOcupadas()
            => Ok(await _dashboardRepositorio.HabitacionesOcupadas());

        [HttpGet("TotalHabitacionesEnLimpieza")]
        public async Task<IActionResult> TotalHabitacionesEnLimpieza()
            => Ok(await _dashboardRepositorio.HabitacionesLimpieza());

        [HttpGet("TotalReservasHoy")]
        public async Task<IActionResult> TotalReservasHoy()
            => Ok(await _dashboardRepositorio.TotalReservasHoy());

        [HttpGet("TotalReservasMes")]
        public async Task<IActionResult> TotalReservasMes()
            => Ok(await _dashboardRepositorio.TotalReservasMes());
    }
}
