using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using SistemaHotel.Server.Repositorio.Implementacion;
using SistemaHotel.Server.Utilidades;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<DbhotelBlazorContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL"));
});

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<IFechaService, FechaService>();

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IRolUsuarioRepositorio, RolUsuarioRepositorio>();
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPisoRepositorio, PisoRepositorio>();
builder.Services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
builder.Services.AddScoped<IHabitacionRepositorio, HabitacionRepositorio>();
builder.Services.AddScoped<IRecepcionRepositorio, RecepcionRepositorio>();
builder.Services.AddScoped<IReservaRepositorio, ReservaRepositorio>();
builder.Services.AddScoped<IReservaGrupoRepositorio, ReservaGrupoRepositorio>();
builder.Services.AddScoped<IDashBoardRepositorio, DashBoardRepositorio>();

// Configuración de autenticación JWT
// El secret se lee desde (en orden): variable de entorno Jwt__Secret > user-secrets > appsettings
var jwtSecret = builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Secret no está configurado o es demasiado débil (mínimo 32 caracteres). " +
        "Configure la variable de entorno 'Jwt__Secret' o use 'dotnet user-secrets'.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5022", "https://localhost:5023")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// NOTA DE SEGURIDAD:
// La migración automática de contraseñas a BCrypt fue eliminada por seguridad.
// Si necesita migrar contraseñas legacy en texto plano, ejecute UNA SOLA VEZ
// el script SQL/utilidad manual fuera de este pipeline (dotnet run con un flag,
// o un endpoint protegido por SuperAdmin), nunca como tarea automática del arranque.

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});

app.UseRouting();

app.UseCors("AllowBlazorClient");

// ✅ IMPORTANTE: si vas a usar [Authorize] en controllers, deben existir:
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();