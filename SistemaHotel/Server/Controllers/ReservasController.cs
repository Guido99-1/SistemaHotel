using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using SistemaHotel.Server.Utilidades;
using SistemaHotel.Shared;
using ReservaReporteDTO = SistemaHotel.Shared.ReservaReporteDTO;


namespace SistemaHotel.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController :ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IReservaRepositorio _reservaRepositorio;

        public ReservasController(IReservaRepositorio reservaRepositorio, IMapper mapper)
        {
            _mapper = mapper;
            _reservaRepositorio = reservaRepositorio;
        }

        [HttpGet]
        [Route("Obtener/{idHabitacion}")]
        public async Task<IActionResult> Obtener(int idHabitacion)
        {
            ResponseDTO<ReservaDTO> _ResponseDTO = new ResponseDTO<ReservaDTO>();

            try
            {
                ReservaDTO Modelo = new ReservaDTO();

                IQueryable<Reserva> query =
                    await _reservaRepositorio.Consultar(r =>
                        r.IdHabitacion == idHabitacion && r.Estado == true);

                query = query
                    .Include(r => r.IdClienteNavigation)
                    .Include(r => r.IdHabitacionNavigation)
                        .ThenInclude(h => h.IdCategoriaNavigation)
                    .Include(r => r.IdHabitacionNavigation)
                        .ThenInclude(h => h.IdPisoNavigation);

                Modelo = _mapper.Map<ReservaDTO>(query.FirstOrDefault());

                _ResponseDTO = new ResponseDTO<ReservaDTO>()
                {
                    status = true,
                    msg = "ok",
                    value = Modelo
                };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<ReservaDTO>()
                {
                    status = false,
                    msg = ex.Message,
                    value = null
                };

                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        [HttpPost]
        [Route("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] ReservaDTO request)
        {
            ResponseDTO<ReservaDTO> _ResponseDTO = new ResponseDTO<ReservaDTO>();

            try
            {
                Reserva _modelo = _mapper.Map<Reserva>(request);
                _modelo.Estado = true;
                _modelo.EstadoReserva = string.IsNullOrEmpty(_modelo.EstadoReserva)
                    ? "CONFIRMADA"
                    : _modelo.EstadoReserva;

                Reserva _modeloCreado = await _reservaRepositorio.Crear(_modelo);

                if (_modeloCreado.IdReserva != 0)
                    _ResponseDTO = new ResponseDTO<ReservaDTO>()
                    {
                        status = true,
                        msg = "ok",
                        value = _mapper.Map<ReservaDTO>(_modeloCreado)
                    };
                else
                    _ResponseDTO = new ResponseDTO<ReservaDTO>()
                    {
                        status = false,
                        msg = "No se pudo crear la reserva"
                    };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                var mensaje = ex.InnerException != null
                    ? ex.InnerException.Message
                    : ex.Message;

                _ResponseDTO = new ResponseDTO<ReservaDTO>()
                {
                    status = false,
                    msg = mensaje
                };

                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        [HttpPut]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] ReservaDTO request)
        {
            ResponseDTO<ReservaDTO> _ResponseDTO = new ResponseDTO<ReservaDTO>();

            try
            {
                Reserva _modelo = _mapper.Map<Reserva>(request);

                bool respuesta = await _reservaRepositorio.Editar(_modelo);

                if (respuesta)
                    _ResponseDTO = new ResponseDTO<ReservaDTO>()
                    {
                        status = true,
                        msg = "ok",
                        value = _mapper.Map<ReservaDTO>(_modelo)
                    };
                else
                    _ResponseDTO = new ResponseDTO<ReservaDTO>()
                    {
                        status = false,
                        msg = "No se pudo editar la reserva"
                    };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<ReservaDTO>()
                {
                    status = false,
                    msg = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        [HttpGet]
        [Route("Reporte")]
        public async Task<IActionResult> Reporte(string? fechaInicio, string? fechaFin)
        {
            ResponseDTO<List<ReservaReporteDTO>> _ResponseDTO =
                new ResponseDTO<List<ReservaReporteDTO>>();

            try
            {
                List<ReservaReporteDTO> listaReporte =
                    _mapper.Map<List<ReservaReporteDTO>>(
                        await _reservaRepositorio.Reporte(fechaInicio, fechaFin)
                    );

                _ResponseDTO = new ResponseDTO<List<ReservaReporteDTO>>()
                {
                    status = true,
                    msg = "ok",
                    value = listaReporte
                };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<List<ReservaReporteDTO>>()
                {
                    status = false,
                    msg = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }
        [HttpGet]
        [Route("Filtrar")]
        public async Task<IActionResult> Filtrar(string fechaInicio, string fechaFin)
        {
            ResponseDTO<List<ReservaDTO>> _ResponseDTO = new();

            try
            {
                var lista = await _reservaRepositorio.Reporte(fechaInicio, fechaFin);

                _ResponseDTO.status = true;
                _ResponseDTO.msg = "ok";
                _ResponseDTO.value = _mapper.Map<List<ReservaDTO>>(lista);

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO.status = false;
                _ResponseDTO.msg = ex.Message;

                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }
        [HttpGet]
        [Route("FiltrarListado")]
        public async Task<IActionResult> FiltrarListado(string fechaInicio, string fechaFin)
        {
            ResponseDTO<List<ReservaReporteDTO>> response = new();

            try
            {
                var lista = await _reservaRepositorio.Reporte(fechaInicio, fechaFin);

                response.status = true;
                response.value = _mapper.Map<List<ReservaReporteDTO>>(lista);
            }
            catch (Exception ex)
            {
                response.status = false;
                response.msg = ex.Message;
            }

            return Ok(response);
        }
        [HttpPut("CambiarEstado/{idReserva}")]
        public async Task<IActionResult> CambiarEstado(int idReserva, [FromBody] string estadoReserva)
        {
            ResponseDTO<bool> resp = new();
            try
            {
                var reserva = await _reservaRepositorio.Obtener(r => r.IdReserva == idReserva);
                if (reserva == null)
                {
                    resp.status = false;
                    resp.msg = "Reserva no encontrada";
                    return NotFound(resp);
                }

                reserva.EstadoReserva = estadoReserva; // "RESERVADA", "CONFIRMADA", "CANCELADA"...
                var ok = await _reservaRepositorio.Editar(reserva);

                resp.status = ok;
                resp.value = ok;
                resp.msg = ok ? "ok" : "No se pudo actualizar";
                return Ok(resp);
            }
            catch (Exception ex)
            {
                resp.status = false;
                resp.msg = ex.Message;
                return StatusCode(500, resp);
            }
        }

        [HttpPut("Cancelar/{idReserva}")]
        public async Task<IActionResult> Cancelar(int idReserva)
        {
            ResponseDTO<bool> resp = new();
            try
            {
                var reserva = await _reservaRepositorio.Obtener(r => r.IdReserva == idReserva);
                if (reserva == null)
                {
                    resp.status = false;
                    resp.msg = "Reserva no encontrada";
                    return NotFound(resp);
                }

                reserva.EstadoReserva = "CANCELADA";
                reserva.Estado = false; // si quieres “anular” lógicamente
                var ok = await _reservaRepositorio.Editar(reserva);

                resp.status = ok;
                resp.value = ok;
                resp.msg = ok ? "ok" : "No se pudo cancelar";
                return Ok(resp);
            }
            catch (Exception ex)
            {
                resp.status = false;
                resp.msg = ex.Message;
                return StatusCode(500, resp);
            }
        }
        //[HttpGet("ExportarPdf")]
        //public async Task<IActionResult> ExportarPdf(string fechaInicio, string fechaFin)
        //{
        //    var lista = await _reservaRepositorio.Reporte(fechaInicio, fechaFin);
        //    var data = _mapper.Map<List<ReservaReporteDTO>>(lista);

        //    // Generar PDF con QuestPDF (ejemplo mínimo)
        //    var pdfBytes = ReservaPdfGenerator.Generar(data, fechaInicio, fechaFin);

        //    return File(pdfBytes, "application/pdf", $"reservas_{fechaInicio}_{fechaFin}.pdf");
        //}



        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {
            ResponseDTO<List<ReservaDTO>> response = new();

            try
            {
                var query = await _reservaRepositorio.Consultar();

                var lista = await query
                    .Include(r => r.IdClienteNavigation)
                    .Include(r => r.IdHabitacionNavigation)
                    .ToListAsync();

                response.status = true;
                response.value = _mapper.Map<List<ReservaDTO>>(lista);
            }
            catch (Exception ex)
            {
                response.status = false;
                response.msg = ex.Message;
            }

            return Ok(response);
        }

    }
}
