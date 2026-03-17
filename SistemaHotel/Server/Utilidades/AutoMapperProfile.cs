using AutoMapper;
using SistemaHotel.Server.Models;
using SistemaHotel.Shared;

namespace SistemaHotel.Server.Utilidades
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region RolUsuario
            CreateMap<Categoria, CategoriaDTO>();
            CreateMap<CategoriaDTO, Categoria>();
            #endregion RolUsuario

            #region RolUsuario
            CreateMap<RolUsuario, RolUsuarioDTO>();
            CreateMap<RolUsuarioDTO, RolUsuario>();
            #endregion RolUsuario


            #region Usuario
            CreateMap<Usuario, UsuarioDTO>();
            CreateMap<UsuarioDTO, Usuario>()
            .ForMember(destino =>
                    destino.Estado,
                    opt => opt.MapFrom(src => true)
                ).ForMember(destino =>
                    destino.IdRolUsuarioNavigation,
                    opt => opt.Ignore()
                );
            #endregion Usuario

            #region Cliente
            CreateMap<Cliente, ClienteDTO>();
            CreateMap<ClienteDTO, Cliente>()
            .ForMember(destino =>
                    destino.Estado,
                    opt => opt.MapFrom(src => true)
                );
            #endregion Cliente

            #region EstadoHabitacion
            CreateMap<EstadoHabitacion, EstadoHabitacionDTO>();
            CreateMap<EstadoHabitacionDTO, EstadoHabitacion>();
            #endregion EstadoHabitacion

            #region Piso
            CreateMap<Piso, PisoDTO>();
            CreateMap<PisoDTO, Piso>();
            #endregion Piso


            #region Habitacion
            CreateMap<Habitacion, HabitacionDTO>();
            CreateMap<HabitacionDTO, Habitacion>()
            .ForMember(destino =>
                    destino.Estado,
                    opt => opt.MapFrom(src => true)
                );
            #endregion Habitacion

            #region Recepcion
            CreateMap<Recepcion, RecepcionDTO>();
            CreateMap<RecepcionDTO, Recepcion>()
            .ForMember(destino =>
                    destino.Estado,
                    opt => opt.MapFrom(src => true)
                );

            CreateMap<Recepcion, ReporteDTO>()
                .ForMember(destino =>
                    destino.NombreCliente,
                    opt => opt.MapFrom(src => src.IdClienteNavigation.NombreCompleto)
                )
                .ForMember(destino =>
                    destino.TipoDocumento,
                    opt => opt.MapFrom(src => src.IdClienteNavigation.TipoDocumento)
                )
                 .ForMember(destino =>
                    destino.NroDocumento,
                    opt => opt.MapFrom(src => src.IdClienteNavigation.Documento)
                )
                  .ForMember(destino =>
                    destino.NroHabitacion,
                    opt => opt.MapFrom(src => src.IdHabitacionNavigation.Numero)
                )
                   .ForMember(destino =>
                    destino.FechaEntrada,
                    opt => opt.MapFrom(src => src.FechaEntrada.Value.ToString("dd/MM/yyyy"))
                )
                     .ForMember(destino =>
                    destino.FechaSalida,
                    opt => opt.MapFrom(src => src.FechaSalida.Value.ToString("dd/MM/yyyy"))
                )
                     .ForMember(destino =>
                    destino.TotalPagado,
                    opt => opt.MapFrom(src => src.TotalPagado.ToString())
                )
                ;
            #endregion Recepcion

            #region Reserva

            // ===========================================
            // Reserva (Server.Model)  ->  ReservaDTO (Shared)
            // ===========================================
            CreateMap<SistemaHotel.Server.Models.Reserva, SistemaHotel.Shared.ReservaDTO>()
                // ids / fechas / montos básicos (AutoMapper los mapea solo, pero ok)
                .ForMember(d => d.IdCliente, opt => opt.MapFrom(s => s.IdCliente))
                .ForMember(d => d.IdHabitacion, opt => opt.MapFrom(s => s.IdHabitacion))

                // string Estado en DTO (tu modelo es bool)
                .ForMember(d => d.Estado, opt => opt.MapFrom(s => s.Estado ? "1" : "0"))

                // EstadoReserva
                .ForMember(d => d.EstadoReserva, opt => opt.MapFrom(s =>
                    string.IsNullOrEmpty(s.EstadoReserva) ? "RESERVADA" : s.EstadoReserva
                ))

                // datos "de lectura" para listado/tabla
                .ForMember(d => d.Cliente, opt => opt.MapFrom(s =>
                    s.IdClienteNavigation != null ? s.IdClienteNavigation.NombreCompleto : ""
                ))
                .ForMember(d => d.Habitacion, opt => opt.MapFrom(s =>
                    s.IdHabitacionNavigation != null ? s.IdHabitacionNavigation.Numero : ""
                ))

                // totales calculados
                .ForMember(d => d.Total, opt => opt.MapFrom(s => s.PrecioInicial ?? 0m))
                .ForMember(d => d.ValorCancelado, opt => opt.MapFrom(s => s.Adelanto ?? 0m))
                .ForMember(d => d.ValorPendiente, opt => opt.MapFrom(s =>
                    (s.PrecioInicial ?? 0m) - (s.Adelanto ?? 0m)
                ))

                // Navigation DTO (si lo necesitas para mostrar/editar)
                // Si no lo usas, puedes ignorarlo.
                .ForMember(d => d.IdClienteNavigation, opt => opt.MapFrom(s => s.IdClienteNavigation));


            // ===========================================
            // ReservaDTO (Shared)  ->  Reserva (Server.Model)  (guardar / editar)
            // ===========================================
            CreateMap<SistemaHotel.Shared.ReservaDTO, SistemaHotel.Server.Models.Reserva>()
                // Estado en tu DTO es string, en modelo es bool
                .ForMember(d => d.Estado, opt => opt.MapFrom(src =>
                    src.Estado != null && (src.Estado.Trim() == "1" || src.Estado.Trim().ToLower() == "true")
                ))
                .ForMember(d => d.EstadoReserva, opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.EstadoReserva) ? "RESERVADA" : src.EstadoReserva
                ))
                // No guardar navigation desde DTO
                .ForMember(d => d.IdClienteNavigation, opt => opt.Ignore())
                .ForMember(d => d.IdHabitacionNavigation, opt => opt.Ignore());


            // ===========================================
            // Reserva -> ReservaReporteDTO  (si lo usas en listado PDF / Reporte)
            // ===========================================
            CreateMap<SistemaHotel.Server.Models.Reserva, SistemaHotel.Shared.ReservaReporteDTO>()
            .ForMember(d => d.IdReserva, opt => opt.MapFrom(s => s.IdReserva))
            .ForMember(d => d.NombreCliente,
                opt => opt.MapFrom(s => s.IdClienteNavigation != null ? s.IdClienteNavigation.NombreCompleto : ""))
            .ForMember(d => d.NroHabitacion,
                opt => opt.MapFrom(s => s.IdHabitacionNavigation != null ? s.IdHabitacionNavigation.Numero : ""))
            .ForMember(d => d.FechaEntrada,
                opt => opt.MapFrom(s => s.FechaEntrada.HasValue ? s.FechaEntrada.Value.ToString("dd/MM/yyyy") : ""))
            .ForMember(d => d.FechaSalida,
                opt => opt.MapFrom(s => s.FechaSalidaReserva.HasValue ? s.FechaSalidaReserva.Value.ToString("dd/MM/yyyy") : ""))
            .ForMember(d => d.Total, opt => opt.MapFrom(s => s.PrecioInicial ?? 0m))
            .ForMember(d => d.ValorCancelado, opt => opt.MapFrom(s => s.Adelanto ?? 0m))
            .ForMember(d => d.ValorPendiente, opt => opt.MapFrom(s => (s.PrecioInicial ?? 0m) - (s.Adelanto ?? 0m)))
            .ForMember(d => d.EstadoReserva, opt => opt.MapFrom(s => s.EstadoReserva))
            .ForMember(d => d.Observacion, opt => opt.MapFrom(s => s.Observacion));

            #endregion


        }
    }
}
