using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaHotel.Server.Repositorio.Contratos;
using SistemaHotel.Shared;

namespace SistemaHotel.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservaGruposController : ControllerBase
    {
        private readonly IReservaGrupoRepositorio _reservaGrupoRepositorio;
        private readonly IMapper _mapper;

        public ReservaGruposController(IReservaGrupoRepositorio reservaGrupoRepositorio, IMapper mapper)
        {
            _reservaGrupoRepositorio = reservaGrupoRepositorio;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("CrearMultiple")]
        public async Task<IActionResult> CrearMultiple([FromBody] CrearReservaMultipleDTO request)
        {
            ResponseDTO<ReservaMultipleResponseDTO> _ResponseDTO = new ResponseDTO<ReservaMultipleResponseDTO>();

            try
            {
                var resultado = await _reservaGrupoRepositorio.CrearReservaMultiple(request);

                _ResponseDTO = new ResponseDTO<ReservaMultipleResponseDTO>()
                {
                    status = true,
                    msg = "Reservas creadas exitosamente",
                    value = resultado
                };

                return StatusCode(StatusCodes.Status201Created, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<ReservaMultipleResponseDTO>()
                {
                    status = false,
                    msg = ex.Message,
                    value = null
                };

                return StatusCode(StatusCodes.Status400BadRequest, _ResponseDTO);
            }
        }

        [HttpGet]
        [Route("Obtener/{idReservaGrupo}")]
        public async Task<IActionResult> Obtener(int idReservaGrupo)
        {
            ResponseDTO<ReservaGrupoDTO> _ResponseDTO = new ResponseDTO<ReservaGrupoDTO>();

            try
            {
                var grupo = await _reservaGrupoRepositorio.ObtenerGrupo(idReservaGrupo);

                if (grupo == null)
                {
                    _ResponseDTO = new ResponseDTO<ReservaGrupoDTO>()
                    {
                        status = false,
                        msg = "Grupo de reserva no encontrado",
                        value = null
                    };
                    return StatusCode(StatusCodes.Status404NotFound, _ResponseDTO);
                }

                var dto = _mapper.Map<ReservaGrupoDTO>(grupo);

                _ResponseDTO = new ResponseDTO<ReservaGrupoDTO>()
                {
                    status = true,
                    msg = "ok",
                    value = dto
                };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<ReservaGrupoDTO>()
                {
                    status = false,
                    msg = ex.Message,
                    value = null
                };

                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        [HttpGet]
        [Route("ObtenerPorCliente/{idCliente}")]
        public async Task<IActionResult> ObtenerPorCliente(int idCliente)
        {
            ResponseDTO<List<ReservaGrupoDTO>> _ResponseDTO = new ResponseDTO<List<ReservaGrupoDTO>>();

            try
            {
                var grupos = await _reservaGrupoRepositorio.ObtenerPorCliente(idCliente);

                var dtos = _mapper.Map<List<ReservaGrupoDTO>>(grupos);

                _ResponseDTO = new ResponseDTO<List<ReservaGrupoDTO>>()
                {
                    status = true,
                    msg = "ok",
                    value = dtos
                };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<List<ReservaGrupoDTO>>()
                {
                    status = false,
                    msg = ex.Message,
                    value = null
                };

                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }
    }
}
