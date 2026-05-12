# 🏨 Sistema de Gestión Hotelera — Hostería Agoyán

[![.NET 7](https://img.shields.io/badge/.NET-7.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor)](https://blazor.net/)
[![MudBlazor](https://img.shields.io/badge/MudBlazor-6.2.2-594AE2)](https://mudblazor.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019%2B-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](license)

Sistema web integral para la gestión operativa de un hotel: reservas, check-ins, check-outs, cobros, dashboard analítico y reportes ejecutivos.

---

## 📋 Tabla de Contenidos

- [Descripción](#-descripción)
- [Características Principales](#-características-principales)
- [Capturas](#-capturas)
- [Arquitectura](#-arquitectura)
- [Stack Tecnológico](#-stack-tecnológico)
- [Requisitos](#-requisitos)
- [Instalación](#-instalación)
- [Configuración](#-configuración)
- [Módulos del Sistema](#-módulos-del-sistema)
- [Roles y Permisos](#-roles-y-permisos)
- [Seguridad](#-seguridad)
- [Documentación](#-documentación)
- [Contribuir](#-contribuir)
- [Licencia](#-licencia)

---

## 📖 Descripción

**Sistema de Gestión Hotelera** es una aplicación web Blazor WebAssembly que centraliza todas las operaciones diarias de un hotel mediano. Permite administrar habitaciones, clientes, reservas (individuales, múltiples y especiales de gerencia), check-ins, check-outs con cálculo automático de penalidades, métodos de pago (incluyendo pago mixto), y generación de reportes ejecutivos en PDF y Excel.

### Objetivo del Sistema
Optimizar la operación del hotel mediante una herramienta moderna, segura y de fácil uso para todos los perfiles del personal: recepción, reservas, gerencia y administración.

### Público Objetivo
- Hoteles pequeños y medianos.
- Hosterías y hospedajes turísticos.
- Establecimientos que requieran control integral de habitaciones y huéspedes.

---

## ✨ Características Principales

### 🎯 Gestión Operativa
- ✅ **Dashboard interactivo** con KPIs en tiempo real, gráficos de ingresos, ocupación y métodos de pago.
- ✅ **Reservas individuales** con validación automática de disponibilidad por rango de fechas.
- ✅ **Reservas múltiples** para grupos: una sola operación cubre varias habitaciones con distribución proporcional de adelantos.
- ✅ **Reservas Gerencia** con tarifa especial fija ($15/persona normal, $20/persona festivo).
- ✅ **Check-in** directo o desde reserva pre-existente.
- ✅ **Check-out** con cálculo automático de penalidades por salida tardía/anticipada.
- ✅ **Pago MIXTO** (combinación de efectivo + transferencia + otros) con desglose detallado.
- ✅ **Reportes** exportables a Excel (ClosedXML) y PDF (jsPDF) con formato profesional.

### 🔐 Seguridad
- 🔒 Autenticación con **JWT** (JSON Web Tokens).
- 🔒 Contraseñas hasheadas con **BCrypt** (work factor 12).
- 🔒 Autorización por roles con `[Authorize]` en frontend y backend.
- 🔒 Transacciones `Serializable` para prevenir race conditions en check-ins simultáneos.
- 🔒 Validaciones defensivas en backend (no se confía en el cliente).
- 🔒 Security headers (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection).
- 🔒 CORS configurado solo para orígenes confiables.

### 🎨 Experiencia de Usuario
- 🌟 **Diseño moderno y pulcro** con paleta corporativa (azul, verde, naranja, gris — sin rojo).
- 🌟 **Responsive** para escritorio, tablet y móvil.
- 🌟 **Componentes consistentes**: KPIs, paneles, tablas, modales, snackbars.
- 🌟 **Confirmaciones** con SweetAlert2.
- 🌟 **Indicadores visuales** de estado: disponible (verde), reservada-hoy (naranja), limpieza (azul), ocupada (gris).

---

## 📸 Capturas

> Las capturas se ubican en `docs/screenshots/` (próximamente).

### Dashboard
*Vista ejecutiva con KPIs, gráficos de ingresos y ocupación por categoría.*

### Recepción
*Mapa visual de habitaciones por piso con código de colores por estado.*

### Reserva Múltiple
*Gestión de reservas grupales con resumen financiero en vivo.*

### Reportes
*Exportación a Excel y PDF con totales, filtros y formato profesional.*

---

## 🏗 Arquitectura

```
SistemaHotel/
├── Client/                     # Blazor WebAssembly (UI)
│   ├── Pages/                  # Páginas Razor
│   │   ├── Modal/              # Modales (cliente, método pago, etc.)
│   │   └── Dashboard.razor     # Tablero ejecutivo
│   ├── Servicios/              # Servicios HTTP cliente
│   │   ├── Contratos/          # Interfaces (IUsuarioServicio, ...)
│   │   └── Implementacion/     # Clases concretas
│   ├── Utilidades/             # Helpers (auth, interceptors)
│   └── Shared/                 # Layout, NavMenu, MainLayout
│
├── Server/                     # ASP.NET Core Web API
│   ├── Controllers/            # Endpoints REST
│   ├── Repositorio/
│   │   ├── Contratos/          # Interfaces
│   │   └── Implementacion/     # Lógica de negocio (EF Core)
│   ├── Models/                 # Entidades de BD
│   ├── Utilidades/             # JWT, BCrypt, AutoMapper, FechaService
│   └── Program.cs              # Configuración del pipeline
│
└── Shared/                     # DTOs compartidos entre Client y Server
```

### Patrón de diseño
- **Repository Pattern** para acceso a datos.
- **DTO Pattern** con AutoMapper para separar entidades de BD del transporte.
- **Dependency Injection** nativo de .NET.
- **JWT Bearer Authentication** stateless.

---

## 🛠 Stack Tecnológico

| Capa | Tecnología |
|---|---|
| **Frontend** | Blazor WebAssembly + MudBlazor 6.2.2 |
| **Backend** | ASP.NET Core 7.0 Web API |
| **Base de Datos** | SQL Server 2019+ |
| **ORM** | Entity Framework Core 7 |
| **Autenticación** | JWT (System.IdentityModel.Tokens.Jwt) |
| **Hashing** | BCrypt.Net-Next (work factor 12) |
| **Mapeo** | AutoMapper 12 |
| **Reportes Excel** | ClosedXML |
| **Reportes PDF** | jsPDF (vía JS Interop) |
| **Alertas** | SweetAlert2 (Blazor) |
| **Iconos** | Material Icons |

---

## ⚙ Requisitos

### Mínimos (Desarrollo)
- **.NET 7 SDK** o superior
- **SQL Server 2019** o LocalDB
- **Visual Studio 2022** (17.4+) o **VS Code** con extensión C#
- **Navegador moderno**: Chrome 110+, Edge 110+, Firefox 110+, Safari 16+
- **RAM**: 4 GB mínimo, 8 GB recomendado
- **Disco**: 1 GB libre

### Producción
- Servidor con **IIS 10** o **Kestrel** detrás de Nginx/Apache.
- **SQL Server** en servidor dedicado o Azure SQL.
- **Certificado SSL** válido (Let's Encrypt o CA comercial).
- **HTTPS obligatorio** en producción.

---

## 🚀 Instalación

### 1. Clonar el repositorio
```bash
git clone https://github.com/Guido99-1/SistemaHotel.git
cd SistemaHotel
```

### 2. Restaurar paquetes NuGet
```bash
dotnet restore
```

### 3. Configurar la base de datos
Ejecuta los scripts SQL en este orden:
```bash
# 1. Crear estructura
sqlcmd -S localhost -i "001_Crear Base datos.txt"

# 2. Insertar datos iniciales
sqlcmd -S localhost -i "002_Insertar Datos.txt"
```

O abre estos archivos en **SQL Server Management Studio (SSMS)** y ejecútalos.

### 4. Configurar la cadena de conexión
Edita `Server/appsettings.json` y ajusta la `CadenaSQL` según tu instalación:
```json
"ConnectionStrings": {
  "CadenaSQL": "Server=(localdb)\\MSSQLLocalDB;Database=DBHotelBlazor;Trusted_Connection=True;..."
}
```

### 5. Configurar el JWT Secret
**No commitear el secret real**. Define la variable de entorno o usa `user-secrets`:
```bash
cd Server
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 48)"
```

Alternativamente, crea `Server/appsettings.Development.json` (que está en `.gitignore`):
```json
{
  "Jwt": {
    "Secret": "tu-clave-aleatoria-minimo-48-caracteres-aqui"
  }
}
```

### 6. Compilar y ejecutar
```bash
dotnet build
dotnet run --project Server
```

Abre tu navegador en `https://localhost:5023` o el puerto configurado.

---

## 🔧 Configuración

### Variables de Entorno (Producción)

| Variable | Descripción | Ejemplo |
|---|---|---|
| `Jwt__Secret` | Clave secreta JWT (mínimo 32 caracteres) | `<openssl rand -base64 48>` |
| `Jwt__Issuer` | Emisor del token | `SistemaHotel` |
| `Jwt__Audience` | Audiencia del token | `SistemaHotelClient` |
| `Jwt__ExpirationMinutes` | Duración del token en minutos | `60` |
| `ConnectionStrings__CadenaSQL` | Cadena de conexión a SQL Server | `Server=...;Database=...;` |
| `TimeZone__Id` | Zona horaria del hotel | `SA Pacific Standard Time` |

### Zona Horaria
El sistema usa `IFechaService` para manejar fechas. Configurar la zona horaria en `appsettings.json`:
```json
"TimeZone": {
  "Id": "SA Pacific Standard Time"
}
```
Opciones comunes: `SA Pacific Standard Time` (EC/CO/PE, UTC-5), `Central Standard Time` (MX, UTC-6).

---

## 📦 Módulos del Sistema

### 1. Dashboard
Tablero ejecutivo con KPIs en tiempo real:
- Total habitaciones, disponibles, ocupadas, en limpieza.
- Ingresos del mes, recepciones del mes, ocupación promedio.
- Gráficos de ingresos diarios, ocupación, métodos de pago.
- Tabs: General, Gerencia.

### 2. Clientes
CRUD completo de huéspedes con:
- Tipo de documento (Cédula / RUC / Pasaporte).
- Búsqueda por nombre, documento, correo.
- Exportación Excel/PDF.

### 3. Mantenimiento (Admin/Gerencia)
- **Categorías**: tipos de habitación (Simple, Doble, Suite, etc.).
- **Pisos**: niveles del hotel.
- **Habitaciones**: número, detalle, precio, piso, categoría.
- **Cambiar Contraseñas**: restablecimiento por administrador.

### 4. Recepción (Admin/Gerencia/Recepcionista)
Mapa visual de habitaciones con código de colores:
- 🟢 Verde — Disponible
- 🟠 Naranja — Reservada hoy
- 🔵 Azul — En limpieza
- ⚫ Gris — Ocupada
- Pestaña "Reservas" para check-in desde reservas activas.

### 5. Reservas (Admin/Gerencia/Recepcionista/Reservas)
- Reservas individuales con validación de disponibilidad.
- Listado con filtros: cliente, habitación, rango de fechas, accesos rápidos (Hoy, Semana, Mes).
- Exportación Excel/PDF.

### 6. Reserva Múltiple
Reservas grupales con:
- Selección de cliente + múltiples habitaciones.
- Distribución proporcional automática de adelantos.
- Validación de disponibilidad por rango.
- Resumen financiero en vivo.

### 7. Reservas Gerencia
Tarifa especial:
- $15 / persona día normal.
- $20 / persona día festivo.
- Pago en su totalidad al crear.

### 8. Salida (Check-out)
- Listado de recepciones activas.
- Cálculo automático de penalidades por salida fuera de fecha.
- Confirmación de salida con método de pago.

### 9. Reportes
- Reporte de Recepciones con filtros avanzados.
- Listado de Reservas exportable.
- Exportación a Excel (ClosedXML) con formato profesional.
- Exportación a PDF (jsPDF) en orientación horizontal con totales.

---

## 👥 Roles y Permisos

| Módulo | Administrador | Gerencia | Recepcionista | Reservas | Empleado |
|---|:---:|:---:|:---:|:---:|:---:|
| Dashboard | ✅ | ✅ | ✅ | ✅ | ✅ |
| Usuarios | ✅ | ✅ | ❌ | ❌ | ❌ |
| Mantenimiento | ✅ | ✅ | ❌ | ❌ | ❌ |
| Cambiar Contraseñas | ✅ | ✅ | ❌ | ❌ | ❌ |
| Clientes | ✅ | ✅ | ✅ | ✅ | ✅ |
| Recepción | ✅ | ✅ | ✅ | ❌ | ❌ |
| Reservas | ✅ | ✅ | ✅ | ✅ | ❌ |
| Reserva Múltiple | ✅ | ✅ | ✅ | ✅ | ❌ |
| Reservas Gerencia | ✅ | ✅ | ✅ | ✅ | ❌ |
| Salida (Check-out) | ✅ | ✅ | ✅ | ❌ | ✅ |
| Reportes | ✅ | ✅ | ✅ | ❌ | ✅ |

---

## 🛡 Seguridad

### Características de seguridad implementadas

| Aspecto | Implementación |
|---|---|
| **Autenticación** | JWT con HS256, expiración configurable, `ClockSkew=30s` |
| **Contraseñas** | BCrypt.Net-Next con work factor 12 |
| **Autorización** | `[Authorize(Roles = "...")]` en frontend y backend |
| **Transacciones** | `IsolationLevel.Serializable` en operaciones críticas (check-in, reserva) |
| **Validación** | Defensiva en backend, no se confía en el cliente |
| **CORS** | Solo orígenes confiables explícitos |
| **Headers** | X-Content-Type-Options, X-Frame-Options, X-XSS-Protection |
| **Secretos** | JWT secret cargado desde variables de entorno / user-secrets |
| **Whitelist** | Estados de reserva validados contra valores permitidos |
| **Fechas pasadas** | Bloqueadas en backend (no solo UI) |

### Buenas prácticas

> ⚠ **NUNCA** committee el archivo `appsettings.Development.json` con secrets reales. Está en `.gitignore`.

> ⚠ **NUNCA** uses la migración automática de contraseñas en producción. Fue removida por seguridad.

> ✅ **Rota el JWT secret** periódicamente (cada 90 días recomendado).

> ✅ **Activa HTTPS obligatorio** en producción (HSTS).

> ✅ **Habilita rate limiting** para prevenir fuerza bruta en `/login`.

---

## 📚 Documentación

Disponible en la raíz del proyecto:

| Archivo | Contenido |
|---|---|
| `Manual_Usuario_SistemaHotel.md` | Manual de usuario completo en Markdown |
| `Manual_Usuario_SistemaHotel.pdf` | Manual en PDF profesional |
| `Manual_Usuario_SistemaHotel.docx` | Manual en Word editable |
| `001_Crear Base datos.txt` | Script de creación de BD |
| `002_Insertar Datos.txt` | Script de datos iniciales (usuarios, roles, categorías) |
| `IMPLEMENTACION_SEGURIDAD_COMPLETADA.md` | Detalle de fixes de seguridad |
| `MIGRACION_CONTRASENAS.md` | Proceso de migración a BCrypt |

### Manual por Rol
El manual de usuario incluye una guía rápida específica para cada rol:
- **Administrador**: configuración inicial, usuarios, supervisión global.
- **Gerencia**: supervisión estratégica, reportes mensuales.
- **Recepcionista**: flujo diario de check-ins, check-outs, cobros.
- **Reservas**: gestión exclusiva de reservas individuales, múltiples y gerenciales.
- **Empleado**: salidas, consulta de clientes y reportes.

---

## 🧪 Ejecutar Tests

```bash
dotnet test
```

> Los tests automatizados están en desarrollo. Por ahora, el sistema cuenta con validaciones defensivas en backend y se ha realizado QA manual exhaustivo (27 casos de prueba documentados).

---

## 🤝 Contribuir

1. Fork del repositorio.
2. Crea una rama (`git checkout -b feature/mi-feature`).
3. Commit de tus cambios (`git commit -m 'Agrega nueva funcionalidad'`).
4. Push a la rama (`git push origin feature/mi-feature`).
5. Abre un Pull Request.

### Convenciones
- Código en **C#** sigue las [convenciones oficiales de Microsoft](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions).
- Razor pages mantienen estilo Dashboard (paleta corporativa, sin colores rojos).
- Commits descriptivos en español o inglés (preferentemente).

---

## 📝 Roadmap

- [ ] Tests unitarios e integración con xUnit.
- [ ] Rate limiting con `AspNetCoreRateLimit`.
- [ ] Refresh tokens para extender sesión.
- [ ] Notificaciones SignalR en tiempo real.
- [ ] App móvil (MAUI o React Native).
- [ ] Integración con pasarela de pago.
- [ ] Exportación de reservas a calendarios (iCal).
- [ ] Multi-idioma (i18n).
- [ ] Multi-hotel (tenant).

---

## 📄 Licencia

Este proyecto está bajo la licencia **MIT**. Ver el archivo [`license`](license) para más detalles.

---

## 👨‍💻 Autor y Contacto

**Hostería Agoyán**
- 📧 Email: agoyanhosteria@gmail.com
- 📞 Teléfono: 0962213000
- 🆔 RUC: 1804742532001

**Desarrollador**
- 🐙 GitHub: [@Guido99-1](https://github.com/Guido99-1)

---

## 🙏 Agradecimientos

- [MudBlazor](https://mudblazor.com/) — Biblioteca de componentes UI.
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) — Generación de Excel.
- [BCrypt.Net-Next](https://github.com/BcryptNet/bcrypt.net) — Hashing seguro.
- [SweetAlert2](https://sweetalert2.github.io/) — Alertas modernas.

---

<div align="center">

**⭐ Si te resulta útil, dale una estrella al repositorio.**

*Hostería Agoyán — Sistema de Gestión Hotelera v1.0 · © 2026*

</div>
