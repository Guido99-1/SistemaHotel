using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using SistemaHotel.Server.Utilidades;
using System.Linq.Expressions;

namespace SistemaHotel.Server.Repositorio.Implementacion
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly DbhotelBlazorContext _dbContext;
        private readonly IPasswordHashingService _passwordHashingService;

        public UsuarioRepositorio(DbhotelBlazorContext dbContext, IPasswordHashingService passwordHashingService)
        {
            _dbContext = dbContext;
            _passwordHashingService = passwordHashingService;
        }


        public async Task<IQueryable<Usuario>> Consultar(Expression<Func<Usuario, bool>> filtro = null)
        {
            IQueryable<Usuario> queryEntidad = filtro == null ? _dbContext.Usuarios : _dbContext.Usuarios.Where(filtro);
            return queryEntidad;
        }

        public async Task<Usuario> Crear(Usuario entidad)
        {
            try
            {
                if (!string.IsNullOrEmpty(entidad.Clave))
                {
                    entidad.Clave = _passwordHashingService.HashPassword(entidad.Clave);
                }
                _dbContext.Set<Usuario>().Add(entidad);
                await _dbContext.SaveChangesAsync();
                return entidad;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Editar(Usuario entidad)
        {
            try
            {
                if (!string.IsNullOrEmpty(entidad.Clave) && !entidad.Clave.StartsWith("$2"))
                {
                    entidad.Clave = _passwordHashingService.HashPassword(entidad.Clave);
                }
                _dbContext.Update(entidad);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(Usuario entidad)
        {
            try
            {
                _dbContext.Remove(entidad);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Usuario>> Lista()
        {
            try
            {
                return await _dbContext.Usuarios.ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<Usuario> Obtener(Expression<Func<Usuario, bool>> filtro = null)
        {
            try
            {
                return await _dbContext.Usuarios.Where(filtro).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }
    }
}
