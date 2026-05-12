using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using SistemaHotel.Server.Utilidades;
using SistemaHotel.Shared;

namespace SistemaHotel.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly IJwtService _jwtService;
        public UsuarioController(IUsuarioRepositorio usuarioRepositorio, IMapper mapper, IPasswordHashingService passwordHashingService, IJwtService jwtService)
        {
            _mapper = mapper;
            _usuarioRepositorio = usuarioRepositorio;
            _passwordHashingService = passwordHashingService;
            _jwtService = jwtService;
        }


        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {
            ResponseDTO<List<UsuarioDTO>> _ResponseDTO = new ResponseDTO<List<UsuarioDTO>>();

            try
            {
                List<UsuarioDTO> ListaUsuarios = new List<UsuarioDTO>();
                IQueryable<Usuario> query = await _usuarioRepositorio.Consultar();
                query = query.Include(r => r.IdRolUsuarioNavigation);

                ListaUsuarios = _mapper.Map<List<UsuarioDTO>>(query.ToList());

                _ResponseDTO = new ResponseDTO<List<UsuarioDTO>>() { status = true, msg = "ok", value = ListaUsuarios };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<List<UsuarioDTO>>() { status = false, msg = ex.Message, value = null };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        [HttpPost]
        [Route("IniciarSesion")]
        [AllowAnonymous]
        public async Task<IActionResult> IniciarSesion([FromBody] LoginRequestDTO loginRequest)
        {
            var _ResponseDTO = new ResponseDTO<LoginResponseDTO>();
            try
            {
                if (!ModelState.IsValid)
                {
                    _ResponseDTO = new ResponseDTO<LoginResponseDTO>() { status = false, msg = "Datos inválidos", value = null };
                    return StatusCode(StatusCodes.Status400BadRequest, _ResponseDTO);
                }

                IQueryable<Usuario> query = await _usuarioRepositorio.Consultar(u => u.Correo == loginRequest.Correo);
                query = query.Include(r => r.IdRolUsuarioNavigation);

                Usuario usuarioEncontrado = query.FirstOrDefault();

                if (usuarioEncontrado != null && _passwordHashingService.VerifyPassword(loginRequest.Clave, usuarioEncontrado.Clave))
                {
                    string token = _jwtService.GenerateToken(usuarioEncontrado.IdUsuario, usuarioEncontrado.Correo, usuarioEncontrado.IdRolUsuarioNavigation.Descripcion);
                    UsuarioDTO usuarioDTO = _mapper.Map<UsuarioDTO>(usuarioEncontrado);

                    var loginResponse = new LoginResponseDTO
                    {
                        Token = token,
                        ExpiresIn = _jwtService.GetTokenExpirationMinutes() * 60,
                        Usuario = usuarioDTO
                    };

                    _ResponseDTO = new ResponseDTO<LoginResponseDTO>() { status = true, msg = "ok", value = loginResponse };
                }
                else
                {
                    _ResponseDTO = new ResponseDTO<LoginResponseDTO>() { status = false, msg = "Credenciales inválidas", value = null };
                }

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<LoginResponseDTO>() { status = false, msg = ex.Message, value = null };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        [HttpPost]
        [Route("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] UsuarioDTO request)
        {
            ResponseDTO<UsuarioDTO> _ResponseDTO = new ResponseDTO<UsuarioDTO>();
            try
            {
                Usuario _usuario = _mapper.Map<Usuario>(request);

                Usuario _usuarioCreado = await _usuarioRepositorio.Crear(_usuario);

                if (_usuarioCreado.IdUsuario != 0)
                    _ResponseDTO = new ResponseDTO<UsuarioDTO>() { status = true, msg = "ok", value = _mapper.Map<UsuarioDTO>(_usuarioCreado) };
                else
                    _ResponseDTO = new ResponseDTO<UsuarioDTO>() { status = false, msg = "No se pudo crear el usuario" };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<UsuarioDTO>() { status = false, msg = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        [HttpPut]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] UsuarioDTO request)
        {
            ResponseDTO<UsuarioDTO> _ResponseDTO = new ResponseDTO<UsuarioDTO>();
            try
            {
                Usuario _usuario = _mapper.Map<Usuario>(request);
                Usuario _usuarioParaEditar = await _usuarioRepositorio.Obtener(u => u.IdUsuario == _usuario.IdUsuario);

                if (_usuarioParaEditar != null)
                {

                    _usuarioParaEditar.NombreCompleto = _usuario.NombreCompleto;
                    _usuarioParaEditar.Correo = _usuario.Correo;
                    _usuarioParaEditar.IdRolUsuario = _usuario.IdRolUsuario;
                    _usuarioParaEditar.Clave = _usuario.Clave;

                    bool respuesta = await _usuarioRepositorio.Editar(_usuarioParaEditar);

                    if (respuesta)
                        _ResponseDTO = new ResponseDTO<UsuarioDTO>() { status = true, msg = "ok", value = _mapper.Map<UsuarioDTO>(_usuarioParaEditar) };
                    else
                        _ResponseDTO = new ResponseDTO<UsuarioDTO>() { status = false, msg = "No se pudo editar el usuario" };
                }
                else
                {
                    _ResponseDTO = new ResponseDTO<UsuarioDTO>() { status = false, msg = "No se encontró el usuario" };
                }

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<UsuarioDTO>() { status = false, msg = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }



        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            ResponseDTO<string> _ResponseDTO = new ResponseDTO<string>();
            try
            {
                Usuario _usuarioEliminar = await _usuarioRepositorio.Obtener(u => u.IdUsuario == id);

                if (_usuarioEliminar != null)
                {

                    bool respuesta = await _usuarioRepositorio.Eliminar(_usuarioEliminar);

                    if (respuesta)
                        _ResponseDTO = new ResponseDTO<string>() { status = true, msg = "ok", value = "" };
                    else
                        _ResponseDTO = new ResponseDTO<string>() { status = false, msg = "No se pudo eliminar el usuario", value = "" };
                }

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<string>() { status = false, msg = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        [HttpPut]
        [Route("CambiarPassword")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDTO request)
        {
            ResponseDTO<string> _ResponseDTO = new ResponseDTO<string>();
            try
            {
                if (!ModelState.IsValid)
                {
                    _ResponseDTO = new ResponseDTO<string>() { status = false, msg = "Datos inválidos", value = "" };
                    return StatusCode(StatusCodes.Status400BadRequest, _ResponseDTO);
                }

                Usuario _usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == request.IdUsuario);

                if (_usuarioEncontrado == null)
                {
                    _ResponseDTO = new ResponseDTO<string>() { status = false, msg = "Usuario no encontrado", value = "" };
                    return StatusCode(StatusCodes.Status404NotFound, _ResponseDTO);
                }

                // Hash de la nueva contraseña
                _usuarioEncontrado.Clave = _passwordHashingService.HashPassword(request.NuevaClave);

                bool respuesta = await _usuarioRepositorio.Editar(_usuarioEncontrado);

                if (respuesta)
                    _ResponseDTO = new ResponseDTO<string>() { status = true, msg = "Contraseña actualizada correctamente", value = "" };
                else
                    _ResponseDTO = new ResponseDTO<string>() { status = false, msg = "No se pudo actualizar la contraseña", value = "" };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<string>() { status = false, msg = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

    }
}
